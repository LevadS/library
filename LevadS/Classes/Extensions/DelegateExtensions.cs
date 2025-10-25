using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

internal static class DelegateTopicExtensions
{
    public static bool CanHandleRequestWithTopic<TRequest, TResponse>(this Delegate del, out string? error)
    {
        error = null;

        // validate delegate return type: must be TResponse or Task<TResponse>
        var returnType = del.Method.ReturnType;
        var expectedReturnType = typeof(Task<>).MakeGenericType(typeof(TResponse));
        if (!(returnType.IsAssignableTo(typeof(TResponse)) || returnType == expectedReturnType))
        {
            var tResp = (typeof(TResponse).FullName ?? typeof(TResponse).Name).ToReadableName();
            var tTaskResp = (expectedReturnType.FullName ?? expectedReturnType.Name).ToReadableName();
            error = $"Delegate must return {tResp} or {tTaskResp}.";

            return false;
        }

        return true;
    }
    
    public static bool CanHandleStreamWithTopic<TRequest, TResponse>(this Delegate del, out string? error)
    {
        error = null;

        // validate delegate return type: must be TResponse or Task<TResponse>
        var returnType = del.Method.ReturnType;
        var expectedReturnType = typeof(IAsyncEnumerable<>).MakeGenericType(typeof(TResponse));
        if (!(returnType == expectedReturnType))
        {
            var readable = (expectedReturnType.FullName ?? expectedReturnType.Name).ToReadableName();
            error = $"Delegate must return {readable}.";
            
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Invoke delegate that returns void. Parameters are resolved from IServiceProvider except:
    /// - parameters decorated with [FromTopic] are taken from topicValues (key = attribute.Name or parameter name)
    /// - parameters of type TMessage (or decorated with [Message]) are bound to provided messageContext.Message
    /// - parameters of type IMessageContext&lt;TMessage&gt; are bound to the provided messageContext if assignable, otherwise resolved from provider
    /// Behavior: prefer FromTopic, then provider, then topic by name, then topic by type, then default value or throw.
    /// </summary>
    public static async Task HandleMessageWithTopicAsync<TMessage>(this Delegate del, IServiceProvider provider, IMessageContext<TMessage> messageContext)
    {
        ArgumentNullException.ThrowIfNull(del);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(messageContext);

        var method = del.Method;
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];
        
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            if (parameterInfo.HandleParametersFromTopic(args, i, messageContext.CapturedValues)) continue;

            if (parameterInfo.HandleImplicitParametersFromTopic(args, i, messageContext.CapturedValues)) continue;

            if (parameterInfo.HandleParameterFromMessage(args, i, (Context)messageContext)) continue;

            if (parameterInfo.HandleMessageContextParameter(args, i, messageContext)) continue;

            if (parameterInfo.HandleParametersFromServiceProvider(args, i, provider)) continue;

            if (parameterInfo.HandleParameterDefaultValue(args, i)) continue;

            throw new InvalidOperationException($"Cannot resolve parameter '{parameterInfo.Name}' of type {parameterInfo.ParameterType}.");
        }
        
        object? invocationResult;
        try
        {
            invocationResult = del.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            throw e.InnerException!;
        }

        if (invocationResult is Task task)
        {
            await task.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Invoke delegate that returns a response. Uses TRequest/TResponse types and IRequestContext&lt;TRequest,TResponse&gt;.
    /// Request and context are taken from requestContext.Request / requestContext.
    /// Uses [Request] attribute to mark request parameter (optional).
    /// </summary>
    public static async Task<TResponse> HandleRequestWithTopicAsync<TRequest, TResponse>(this Delegate del, IServiceProvider provider, IRequestContext<TRequest> requestContext)
    {
        ArgumentNullException.ThrowIfNull(del);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(requestContext);

        var method = del.Method;
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            if (parameterInfo.HandleParametersFromTopic(args, i, requestContext.CapturedValues)) continue;

            if (parameterInfo.HandleImplicitParametersFromTopic(args, i, requestContext.CapturedValues)) continue;

            if (parameterInfo.HandleParameterFromRequest<TRequest, TResponse>(args, i, requestContext)) continue;

            if (parameterInfo.HandleRequestContextParameter<TRequest, TResponse>(args, i, requestContext)) continue;

            if (parameterInfo.HandleParametersFromServiceProvider(args, i, provider)) continue;

            if (parameterInfo.HandleParameterDefaultValue(args, i)) continue;

            throw new InvalidOperationException($"Cannot resolve parameter '{parameterInfo.Name}' of type {parameterInfo.ParameterType}.");
        }

        object? invocationResult;
        try
        {
            invocationResult = del.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            throw e.InnerException!;
        }

        switch (invocationResult)
        {
            // sync return
            case TResponse resp:
                return resp;
            
            // async Task<TResponse>
            case Task<TResponse> taskOfT:
                return await taskOfT.ConfigureAwait(false);
            
            // generic Task (wrong shape) - await then return default
            case Task t:
                await t.ConfigureAwait(false);
                return default!;
        }

        // try to coerce (if not null)
        if (invocationResult is TResponse result)
        {
            return result;
        }

        return default!;
    }

    /// <summary>
    /// Invoke delegate that returns a response. Uses TRequest/TResponse types and IRequestContext&lt;TRequest,TResponse&gt;.
    /// Request and context are taken from requestContext.Request / requestContext.
    /// Uses [Request] attribute to mark request parameter (optional).
    /// </summary>
    public static async IAsyncEnumerable<TResponse> HandleStreamWithTopicAsync<TRequest, TResponse>(this Delegate del, IServiceProvider provider, IStreamContext<TRequest> streamContext)
    {
        ArgumentNullException.ThrowIfNull(del);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(streamContext);

        var method = del.Method;
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterInfo = parameters[i];

            if (parameterInfo.HandleParametersFromTopic(args, i, streamContext.CapturedValues)) continue;

            if (parameterInfo.HandleParameterFromRequest<TRequest, TResponse>(args, i, streamContext)) continue;

            if (parameterInfo.HandleRequestContextParameter<TRequest, TResponse>(args, i, streamContext)) continue;

            if (parameterInfo.HandleParametersFromServiceProvider(args, i, provider)) continue;

            if (parameterInfo.HandleImplicitParametersFromTopic(args, i, streamContext.CapturedValues)) continue;

            if (parameterInfo.HandleParameterDefaultValue(args, i)) continue;

            throw new InvalidOperationException($"Cannot resolve parameter '{parameterInfo.Name}' of type {parameterInfo.ParameterType}.");
        }
        
        object? invocationResult;
        try
        {
            invocationResult = del.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            throw e.InnerException!;
        }

        if (invocationResult is not IAsyncEnumerable<TResponse> asyncEnumerable)
        {
            throw new InvalidOperationException($"Stream handler did not returned value of type {typeof(IAsyncEnumerable<TResponse>).Name}");
        }
        
        await foreach (var item in asyncEnumerable)
        {
            yield return item;
        }
    }
}
using System.Reflection;
using LevadS.Attributes;
using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

internal static class ParameterInfoExtensions
{
    public static bool HandleParameterFromMessage(this ParameterInfo p, object?[] args, int i, Context context)
    {
        if (p.ParameterType.IsAssignableFrom(context.MessageType))
        {
            args[i] = context.MessageObject;
            return true;
        }
        
        var reqAttr = p.GetCustomAttribute<FromMessageAttribute>(false);
        if (reqAttr != null)
        {
            throw new InvalidOperationException(
                $"Invalid type of parameter '{p.Name}': must be of type {context.MessageType.Name}.");
        }

        return false;
    }

    public static bool HandleParameterFromRequest<TRequest, TResponse>(this ParameterInfo p, object?[] args, int i, IRequestContext<TRequest> requestContext)
    {
        if (p.ParameterType.IsAssignableFrom(typeof(TRequest)))
        {
            args[i] = requestContext.Request;
            return true;
        }
        
        var reqAttr = p.GetCustomAttribute<FromRequestAttribute>(false);
        if (reqAttr != null)
        {
            throw new InvalidOperationException($"Invalid type of parameter '{p.Name}': must be of type {typeof(TRequest).Name}.");
        }

        return false;
    }

    public static bool HandleParameterFromRequest<TRequest, TResponse>(this ParameterInfo p, object?[] args, int i, IStreamContext<TRequest> streamContext)
    {
        if (p.ParameterType.IsAssignableFrom(typeof(TRequest)))
        {
            args[i] = streamContext.Request;
            return true;
        }
        
        var reqAttr = p.GetCustomAttribute<FromRequestAttribute>(false);
        if (reqAttr != null)
        {
            throw new InvalidOperationException($"Invalid type of parameter '{p.Name}': must be of type {typeof(TRequest).Name}.");
        }

        return false;
    }

    public static bool HandleMessageContextParameter(this ParameterInfo p, object?[] args, int i, IContext context)
    {
        if (p.ParameterType.IsInstanceOfType(context))
        {
            args[i] = context;
            return true;
        }

        return false;
    }

    public static bool HandleRequestContextParameter<TRequest, TResponse>(this ParameterInfo p, object?[] args, int i, IRequestContext<TRequest> requestContext)
    {
        if (p.ParameterType.IsInstanceOfType(requestContext))
        {
            args[i] = requestContext;
            return true;
        }

        return false;
    }

    public static bool HandleRequestContextParameter<TRequest, TResponse>(this ParameterInfo p, object?[] args, int i, IStreamContext<TRequest> requestContext)
    {
        if (p.ParameterType.IsInstanceOfType(requestContext))
        {
            args[i] = requestContext;
            return true;
        }

        return false;
    }

    public static bool HandleParameterDefaultValue(this ParameterInfo p, object?[] args, int i)
    {
        if (p.HasDefaultValue)
        {
            args[i] = p.DefaultValue;
            return true;
        }

        return false;
    }

    public static bool HandleParametersFromServiceProvider(this ParameterInfo p, object?[] args, int i, IServiceProvider provider)
    {
        var service = provider.GetService(p.ParameterType);
        if (service != null)
        {
            args[i] = service;
            return true;
        }

        return false;
    }

    public static bool HandleImplicitParametersFromTopic(this ParameterInfo p, object?[] args, int i, IReadOnlyDictionary<string, object> topicLookup)
    {
        var paramName = p.Name;
        if (!string.IsNullOrEmpty(paramName) && topicLookup.TryGetValue(paramName, out var rawByName))
        {
            if (!rawByName.TryConvert(p.ParameterType, out var convByName))
                throw new InvalidOperationException($"Cannot convert topic value '{paramName}' to parameter type {p.ParameterType}.");
            args[i] = convByName;
            return true;
        }

        return false;
    }

    public static bool HandleParametersFromTopic(this ParameterInfo p, object?[] args, int i, IReadOnlyDictionary<string, object> topicLookup)
    {
        var fromTopic = p.GetCustomAttribute<FromTopicAttribute>(false);
        if (fromTopic != null)
        {
            var key = string.IsNullOrEmpty(fromTopic.Name)
                ? p.Name ?? string.Empty
                : fromTopic.Name;
            
            if (topicLookup.TryGetValue(key, out var raw))
            {
                if (!raw.TryConvert(p.ParameterType, out var conv))
                    throw new InvalidOperationException($"Cannot convert topic value '{key}' to parameter type {p.ParameterType}.");
                args[i] = conv;
                return true;
            }
            
            if (p.HasDefaultValue)
            {
                args[i] = p.DefaultValue;
                return true;
            }
            
            throw new InvalidOperationException($"Missing topic value for parameter '{p.Name}'.");
        }

        return false;
    }
}
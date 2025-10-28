using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using LevadS.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

internal static class ServiceProviderExtensions
{
    // #region GetTopicMessageHandlers
    // internal static ITopicMessageHandler<TMessage>? GetTopicMessageHandler<TMessage>(this IServiceProvider serviceProvider, MessageContext<TMessage> messageContext)
    //     => GetTopicMessageHandlers<TMessage>(serviceProvider, messageContext).FirstOrDefault();
    //
    // internal static IEnumerable<ITopicMessageHandler<TMessage>> GetTopicMessageHandlers<TMessage>(
    //     this IServiceProvider serviceProvider, MessageContext<TMessage> messageContext)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantMessageServices<TMessage, ITopicMessageHandler<TMessage>>(),
    //         messageContext.CloneInstance,
    //         (h, ctx) => messageContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    //
    // private static readonly ConcurrentDictionary<(Type, Type), List<Type>> MessageServiceTypesCache = new();
    //
    // private static TService[] GetVariantMessageServices<TMessage, TService>(
    //     this IServiceProvider serviceProvider, string[]? keys = null)
    //     where TService : class
    // {
    //     var interfaceType = typeof(TService);
    //     var genericInterfaceType = interfaceType.GetGenericTypeDefinition();
    //     
    //     var cachedServiceTypes = GetVariantMessageServiceTypes<TMessage, TService>();
    //
    //     var envelopeFactory = (object? service)
    //         => service?.GetType().IsAssignableTo(interfaceType) switch
    //         {
    //             null => default,
    //             true => (TService)service,
    //             false when genericInterfaceType == typeof(ITopicMessageDispatchFilter<>)
    //                 => (TService)(object)new TopicMessageDispatchFilterVariantWrapper<TMessage>(service),
    //             false when genericInterfaceType == typeof(ITopicMessageHandlingFilter<>)
    //                 => (TService)(object)new TopicMessageFilterVariantWrapper<TMessage>(service),
    //             _ => default
    //         };
    //         
    //     return cachedServiceTypes
    //         .SelectMany(i => 
    //             (
    //                 keys != null 
    //                     ? keys.SelectMany(key => serviceProvider.GetKeyedServices(i!, key))
    //                     : serviceProvider.GetServices(i!)
    //             )
    //             .Select(envelopeFactory)
    //             .Where(s => s != null)
    //             .Select(s => s!)
    //         )
    //         .ToArray();
    // }
    //
    // internal static List<Type> GetVariantMessageServiceTypes<TMessage, TService>()
    //     where TService : class
    // {
    //     return MessageServiceTypesCache.GetOrAdd(
    //         (typeof(TMessage), typeof(TService)),
    //         static key =>
    //         {
    //             var messageType = key.Item1;
    //             var interfaceType = key.Item2;
    //             var genericInterfaceType = interfaceType.GetGenericTypeDefinition();
    //             
    //             List<Type> messageTypes = [];
    //
    //             messageTypes.AddRange(messageType.GetInterfaces());
    //             while (messageType != null)
    //             {
    //                 messageTypes.Add(messageType);
    //                 messageType = messageType.BaseType;
    //             }
    //
    //             var serviceTypes = messageTypes
    //                 .Select(m =>
    //                 {
    //                     try
    //                     {
    //                         return genericInterfaceType.MakeGenericType(m);
    //                     }
    //                     catch (Exception)
    //                     {
    //                         return null;
    //                     }
    //                 })
    //                 .Where(i => i != null)
    //                 .Select(i => i!);
    //
    //             if (genericInterfaceType == typeof(IMessageHandler<>))
    //             {
    //                 serviceTypes = serviceTypes.Where(i => i.IsAssignableTo(interfaceType));
    //             }
    //
    //             return serviceTypes.ToList();
    //         }
    //     );
    // }
    // #endregion
    //
    // #region GetTopicRequestHandlers
    // internal static ITopicRequestHandler<TRequest, TResponse>? GetTopicRequestHandler<TRequest, TResponse>(this IServiceProvider serviceProvider, RequestContext<TRequest> requestContext)
    //     => GetTopicRequestHandlers<TRequest, TResponse>(serviceProvider, requestContext).FirstOrDefault();
    //
    // private static IEnumerable<ITopicRequestHandler<TRequest, TResponse>> GetTopicRequestHandlers<TRequest, TResponse>(this IServiceProvider serviceProvider, RequestContext<TRequest> requestContext)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantRequestServices<TRequest, TResponse, ITopicRequestHandler<TRequest, TResponse>>(),
    //         requestContext.CloneInstance,
    //         (h, ctx) => requestContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    //
    // private static readonly ConcurrentDictionary<(Type, Type, Type), List<Type>> RequestServiceTypesCache = new();
    //
    // private static TService[] GetVariantRequestServices<TRequest, TResponse, TService>(
    //     this IServiceProvider serviceProvider, string[]? keys = null)
    //     where TService : class
    // {
    //     var interfaceType = typeof(TService);
    //     var genericInterfaceType = interfaceType.GetGenericTypeDefinition();
    //
    //     var cachedServiceTypes = GetVariantRequestServiceTypes<TRequest, TResponse, TService>();
    //
    //     var envelopeFactory = (object? service)
    //         => service?.GetType().IsAssignableTo(interfaceType) switch
    //         {
    //             null => default,
    //             true => (TService)service,
    //             false when genericInterfaceType == typeof(ITopicRequestDispatchFilter<,>)
    //                 => (TService)(object)new TopicRequestDispatchFilterVariantWrapper<TRequest, TResponse>(service),
    //             false when genericInterfaceType == typeof(ITopicRequestHandlingFilter<,>)
    //                 => (TService)(object)new TopicRequestHandlingFilterVariantWrapper<TRequest, TResponse>(service),
    //             false when genericInterfaceType == typeof(ITopicStreamDispatchFilter<,>)
    //                 => (TService)(object)new TopicStreamDispatchFilterVariantWrapper<TRequest, TResponse>(service),
    //             false when genericInterfaceType == typeof(ITopicStreamHandlingFilter<,>)
    //                 => (TService)(object)new TopicStreamHandlingFilterVariantWrapper<TRequest, TResponse>(service),
    //             _ => default
    //         };
    //
    //     return cachedServiceTypes
    //         .SelectMany(i =>
    //             (
    //                 keys != null 
    //                     ? keys.SelectMany(key => serviceProvider.GetKeyedServices(i, key))
    //                     : serviceProvider.GetServices(i)
    //             )
    //             .Select(envelopeFactory)
    //             .Where(s => s != null)
    //             .Select(s => s!)
    //         )
    //         .ToArray();
    // }
    //
    // internal static List<Type> GetVariantRequestServiceTypes<TRequest, TResponse, TService>()
    //     where TService : class
    // {
    //     return RequestServiceTypesCache.GetOrAdd(
    //         (typeof(TRequest), typeof(TResponse), typeof(TService)),
    //         static key =>
    //         {
    //             var requestType = key.Item1;
    //             var responseType = key.Item2;
    //             var interfaceType = key.Item3;
    //             var genericInterfaceType = interfaceType.GetGenericTypeDefinition();
    //             
    //             List<Type> requestTypes = [];
    //             List<Type> responseTypes = [];
    //
    //             requestTypes.AddRange(requestType.GetInterfaces());
    //             while (requestType != null)
    //             {
    //                 requestTypes.Add(requestType);
    //                 requestType = requestType.BaseType;
    //             }
    //
    //             responseTypes.AddRange(responseType.GetInterfaces());
    //             while (responseType != null)
    //             {
    //                 responseTypes.Add(responseType);
    //                 responseType = responseType.BaseType;
    //             }
    //
    //             var serviceTypes = requestTypes
    //                 .SelectMany(requestVariantType =>
    //                     responseTypes.Select(responseVariantType =>
    //                     {
    //                         try
    //                         {
    //                             return genericInterfaceType.MakeGenericType(requestVariantType, responseVariantType);
    //                         }
    //                         catch (Exception)
    //                         {
    //                             return null;
    //                         }
    //                     })
    //                 )
    //                 .Where(i => i != null)
    //                 .Select(i => i!);
    //
    //             if (genericInterfaceType == typeof(IRequestHandler<,>) || genericInterfaceType == typeof(IStreamHandler<,>))
    //             {
    //                 serviceTypes = serviceTypes.Where(i => i.IsAssignableTo(interfaceType));
    //             }
    //
    //             return serviceTypes.ToList();
    //         }
    //     );
    // }
    //
    // #endregion
    //
    // #region GetTopicStreamHandlers
    // internal static ITopicStreamHandler<TRequest, TResponse>? GetTopicStreamHandler<TRequest, TResponse>(this IServiceProvider serviceProvider, StreamContext<TRequest> requestContext)
    //     => GetTopicStreamHandlers<TRequest, TResponse>(serviceProvider, requestContext).FirstOrDefault();
    //
    // private static IEnumerable<ITopicStreamHandler<TRequest, TResponse>> GetTopicStreamHandlers<TRequest, TResponse>(this IServiceProvider serviceProvider, StreamContext<TRequest> requestContext)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantRequestServices<TRequest, TResponse, ITopicStreamHandler<TRequest, TResponse>>(),
    //         requestContext.CloneInstance,
    //         (h, ctx) => requestContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    //
    // #endregion
    //
    // #region GetTopicMessageHandlingFilters
    // internal static IEnumerable<ITopicMessageDispatchFilter<TMessage>> GetTopicMessageDispatchFilters<TMessage>(
    //     this IServiceProvider serviceProvider, MessageContext<TMessage> messageContext)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantMessageServices<TMessage, ITopicMessageDispatchFilter<TMessage>>(),
    //         messageContext.CloneInstance,
    //         (h, ctx) => messageContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    //
    // internal static IEnumerable<ITopicMessageHandlingFilter<TMessage>> GetTopicMessageHandlingFilters<TMessage>(
    //     this IServiceProvider serviceProvider, MessageContext<TMessage> messageContext, string key)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantMessageServices<TMessage, ITopicMessageHandlingFilter<TMessage>>(["global", key]),
    //         messageContext.CloneInstance,
    //         (h, ctx) => messageContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    // #endregion
    //
    //
    // #region GetTopicRequestHandlingFilters
    // internal static IEnumerable<ITopicRequestDispatchFilter<TRequest, TResponse>> GetTopicRequestDispatchFilters<TRequest, TResponse>(
    //     this IServiceProvider serviceProvider, RequestContext<TRequest> messageContext)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantRequestServices<TRequest, TResponse, ITopicRequestDispatchFilter<TRequest, TResponse>>(),
    //         messageContext.CloneInstance,
    //         (h, ctx) => messageContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    //
    // internal static IEnumerable<ITopicRequestHandlingFilter<TRequest, TResponse>> GetTopicRequestHandlingFilters<TRequest, TResponse>(
    //     this IServiceProvider serviceProvider, RequestContext<TRequest> context, string key)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantRequestServices<TRequest, TResponse, ITopicRequestHandlingFilter<TRequest, TResponse>>(["global", key]),
    //         () => context.CloneInstance(),
    //         (h, ctx) => context.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    // #endregion
    //
    //
    // #region GetTopicStreamHandlingFilters
    // internal static IEnumerable<ITopicStreamDispatchFilter<TRequest, TResponse>> GetTopicStreamDispatchFilters<TRequest, TResponse>(
    //     this IServiceProvider serviceProvider, StreamContext<TRequest> messageContext)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantRequestServices<TRequest, TResponse, ITopicStreamDispatchFilter<TRequest, TResponse>>(),
    //         messageContext.CloneInstance,
    //         (h, ctx) => messageContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    //
    // internal static IEnumerable<ITopicStreamHandlingFilter<TRequest, TResponse>> GetTopicStreamHandlingFilters<TRequest, TResponse>(
    //     this IServiceProvider serviceProvider, StreamContext<TRequest> streamContext, string key)
    //     => AssignCloneAndFilter(
    //         serviceProvider.GetVariantRequestServices<TRequest, TResponse, ITopicStreamHandlingFilter<TRequest, TResponse>>(["global", key]),
    //         () => streamContext.CloneInstance(),
    //         (h, ctx) => streamContext.Topic!.MatchesTopicPattern(h.TopicPattern, ctx.CapturedTopicValues),
    //         (h, ctx) => h.Context = ctx
    //     );
    // #endregion
    //
    /// <summary>
    /// Creates instance of given type. Constructor parameters are resolved from IServiceProvider,
    /// except parameters decorated with [FromTopic] are taken from topicValues dictionary (key is attribute.Name or parameter name).
    /// Additionally, for parameters without [FromTopic], the method will try to bind a topic value by parameter name
    /// (case-insensitive) and by type when provider cannot supply the service.
    /// Parameters with default values are honored when neither provider nor topic supplies a value.
    /// Behavior is similar to ActivatorUtilities.CreateInstance but supports topic-supplied values.
    /// </summary>
    internal static object CreateInstanceWithTopic(this IServiceProvider provider, Type type, IContext context)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(context);

        // return ActivatorUtilities.CreateInstance(provider, type);
    
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        ConstructorInfo? bestConstructor = null;
        object[]? bestArgs = null;
        var bestParamCount = -1;
    
        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            var ok = !parameters
                .Where((parameterInfo, i) =>
                    !parameterInfo.HandleParametersFromTopic(args, i, context.CapturedValues) &&
                    !parameterInfo.HandleMessageContextParameter(args, i, context) &&
                    !parameterInfo.HandleParametersFromServiceProvider(args, i, provider) &&
                    !parameterInfo.HandleImplicitParametersFromTopic(args, i, context.CapturedValues) &&
                    !parameterInfo.HandleParameterDefaultValue(args, i))
                .Any();
    
            if (!ok) continue;
    
            // prefer ctor with more parameters (like ActivatorUtilities)
            if (parameters.Length > bestParamCount)
            {
                bestParamCount = parameters.Length;
                bestConstructor = ctor;
                bestArgs = args!;
            }
        }
    
        if (bestConstructor == null)
        {
            throw new InvalidOperationException($"No suitable public constructor found for type {type.FullName} that can be satisfied by the service provider and topic values.");
        }
    
        return bestConstructor.Invoke(bestArgs);
    }
    //
    // // internal static T CreateInstanceWithTopic<T>(this IServiceProvider provider, TopicServiceContainer.TopicServiceResolutionContext resolutionContext)
    // //     => (T)CreateInstanceWithTopic(provider, typeof(T), resolutionContext);
    // //
    // // internal static object CreateInstanceWithTopic(this IServiceProvider provider, Type type, TopicServiceContainer.TopicServiceResolutionContext resolutionContext)
    // // {
    // //     ArgumentNullException.ThrowIfNull(provider);
    // //     ArgumentNullException.ThrowIfNull(type);
    // //     ArgumentNullException.ThrowIfNull(resolutionContext);
    // //
    // //     var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    // //     ConstructorInfo? bestConstructor = null;
    // //     object[]? bestArgs = null;
    // //     var bestParamCount = -1;
    // //
    // //     foreach (var ctor in constructors)
    // //     {
    // //         var parameters = ctor.GetParameters();
    // //         var args = new object?[parameters.Length];
    // //         var ok = !parameters
    // //             .Where((parameterInfo, i) =>
    // //                 !parameterInfo.HandleParametersFromTopic(args, i, resolutionContext.CapturedValues) &&
    // //                 !parameterInfo.HandleParametersFromServiceProvider(args, i, provider) &&
    // //                 !parameterInfo.HandleImplicitParametersFromTopic(args, i, resolutionContext.CapturedValues) &&
    // //                 !parameterInfo.HandleParameterDefaultValue(args, i))
    // //             .Any();
    // //
    // //         if (!ok) continue;
    // //
    // //         // prefer ctor with more parameters (like ActivatorUtilities)
    // //         if (parameters.Length > bestParamCount)
    // //         {
    // //             bestParamCount = parameters.Length;
    // //             bestConstructor = ctor;
    // //             bestArgs = args!;
    // //         }
    // //     }
    // //
    // //     if (bestConstructor == null)
    // //     {
    // //         throw new InvalidOperationException($"No suitable public constructor found for type {type.FullName} that can be satisfied by the service provider and topic values.");
    // //     }
    // //
    // //     return bestConstructor.Invoke(bestArgs);
    // // }
    //
    // private static IEnumerable<T> AssignCloneAndFilter<T, TContext>(
    //     IEnumerable<T> items,
    //     Func<TContext> cloneFactory,
    //     Func<T, TContext, bool> matches,
    //     Action<T, TContext> assign)
    // {
    //     foreach (var item in items)
    //     {
    //         var ctx = cloneFactory();
    //         assign(item, ctx);
    //         if (matches(item, ctx))
    //         {
    //             yield return item;
    //         }
    //     }
    // }
    //
    //

    internal static T CreateInstanceWithTopic<T>(this IServiceProvider provider, IContext context)
        => (T)CreateInstanceWithTopic(provider, typeof(T), context);
}
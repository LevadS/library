using System.Collections.Concurrent;
using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

internal static class ServiceResolverExtensions
{
    #region GetMessageHandlers
    internal static (IMessageHandler<TMessage> Service, IMessageContext<TMessage> Context)? GetMessageHandler<TMessage>(this IServiceResolver serviceResolver, IMessageContext<TMessage> messageContext)
        => GetMessageHandlers<TMessage>(serviceResolver, messageContext).FirstOrDefault();
    
    internal static IEnumerable<(IMessageHandler<TMessage> Service, IMessageContext<TMessage> Context)> GetMessageHandlers<TMessage>(
        this IServiceResolver serviceResolver, IMessageContext<TMessage> messageContext)
        => serviceResolver
            .GetServices<IMessageHandler<TMessage>, TMessage, object>(messageContext)
            .Select(t => (t.Item1, (IMessageContext<TMessage>)t.Item2));

    private static readonly ConcurrentDictionary<(Type, Type), List<Type>> MessageServiceTypesCache = new();

    internal static List<Type> GetVariantMessageServiceTypes<TMessage, TService>()
        where TService : class
    {
        return MessageServiceTypesCache.GetOrAdd(
            (typeof(TMessage), typeof(TService)),
            static key =>
            {
                var messageType = key.Item1;
                var interfaceType = key.Item2;
                var genericInterfaceType = interfaceType.GetGenericTypeDefinition();
                
                List<Type> messageTypes = [];

                messageTypes.AddRange(messageType.GetInterfaces());
                while (messageType != null)
                {
                    messageTypes.Add(messageType);
                    messageType = messageType.BaseType;
                }

                var serviceTypes = messageTypes
                    .Select(m =>
                    {
                        try
                        {
                            return genericInterfaceType.MakeGenericType(m);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    })
                    .Where(i => i != null)
                    .Select(i => i!);

                if (genericInterfaceType == typeof(IMessageHandler<>))
                {
                    serviceTypes = serviceTypes.Where(i => i.IsAssignableTo(interfaceType));
                }

                return serviceTypes.ToList();
            }
        );
    }
    #endregion

    #region GetRequestHandlers
    internal static (IRequestHandler<TRequest, TResponse> Service, IRequestContext<TRequest> Context)? GetRequestHandler<TRequest, TResponse>(this IServiceResolver serviceResolver, IRequestContext<TRequest> requestContext)
        => GetRequestHandlers<TRequest, TResponse>(serviceResolver, requestContext).FirstOrDefault();
    
    private static IEnumerable<(IRequestHandler<TRequest, TResponse> Service, IRequestContext<TRequest> Context)> GetRequestHandlers<TRequest, TResponse>(this IServiceResolver serviceResolver, IRequestContext<TRequest> requestContext)
        => serviceResolver
            .GetServices<IRequestHandler<TRequest, TResponse>, TRequest, TResponse>(requestContext)
            .Select(t => (t.Item1, (IRequestContext<TRequest>)t.Item2));
    
    private static readonly ConcurrentDictionary<(Type, Type, Type), List<Type>> RequestServiceTypesCache = new();

    internal static List<Type> GetVariantRequestServiceTypes<TRequest, TResponse, TService>()
        where TService : class
    {
        return RequestServiceTypesCache.GetOrAdd(
            (typeof(TRequest), typeof(TResponse), typeof(TService)),
            static key =>
            {
                var requestType = key.Item1;
                var responseType = key.Item2;
                var interfaceType = key.Item3;
                var genericInterfaceType = interfaceType.GetGenericTypeDefinition();
                
                List<Type> requestTypes = [];
                List<Type> responseTypes = [];

                requestTypes.AddRange(requestType.GetInterfaces());
                while (requestType != null)
                {
                    requestTypes.Add(requestType);
                    requestType = requestType.BaseType;
                }

                responseTypes.AddRange(responseType.GetInterfaces());
                while (responseType != null)
                {
                    responseTypes.Add(responseType);
                    responseType = responseType.BaseType;
                }

                var serviceTypes = requestTypes
                    .SelectMany(requestVariantType =>
                        responseTypes.Select(responseVariantType =>
                        {
                            try
                            {
                                return genericInterfaceType.MakeGenericType(requestVariantType, responseVariantType);
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        })
                    )
                    .Where(i => i != null)
                    .Select(i => i!);

                if (genericInterfaceType == typeof(IRequestHandler<,>) || genericInterfaceType == typeof(IStreamHandler<,>))
                {
                    serviceTypes = serviceTypes.Where(i => i.IsAssignableTo(interfaceType));
                }

                return serviceTypes.ToList();
            }
        );
    }

    #endregion

    #region GetStreamHandlers
    internal static (IStreamHandler<TRequest, TResponse> Service, IStreamContext<TRequest> Context)? GetStreamHandler<TRequest, TResponse>(this IServiceResolver serviceResolver, IStreamContext<TRequest> streamContext)
        => GetStreamHandlers<TRequest, TResponse>(serviceResolver, streamContext).FirstOrDefault();
    
    private static IEnumerable<(IStreamHandler<TRequest, TResponse> Service, IStreamContext<TRequest> Context)> GetStreamHandlers<TRequest, TResponse>(this IServiceResolver serviceResolver, IStreamContext<TRequest> streamContext)
        => serviceResolver
            .GetServices<IStreamHandler<TRequest, TResponse>, TRequest, TResponse>(streamContext)
            .Select(t => (t.Item1, (IStreamContext<TRequest>)t.Item2));

    #endregion
    
    #region GetMessageHandlingFilters
    internal static IEnumerable<(IMessageDispatchFilter<TMessage> Service, IMessageContext<TMessage> Context)> GetMessageDispatchFilters<TMessage>(
        this IServiceResolver serviceResolver, IMessageContext<TMessage> messageContext)
        => serviceResolver
            .GetServices<IMessageDispatchFilter<TMessage>, TMessage, object>(messageContext)
            .Select(t => (t.Item1, (IMessageContext<TMessage>)t.Item2));

    internal static IEnumerable<(IMessageHandlingFilter<TMessage> Service, IMessageContext<TMessage> Context)> GetMessageHandlingFilters<TMessage>(
        this IServiceResolver serviceResolver, IMessageContext<TMessage> messageContext, string key)
        => serviceResolver
            .GetServices<IMessageHandlingFilter<TMessage>, TMessage, object>(messageContext, ["global", key])
            .Select(t => (t.Item1, (IMessageContext<TMessage>)t.Item2));
    #endregion
    

    #region GetRequestHandlingFilters
    internal static IEnumerable<(IRequestDispatchFilter<TRequest, TResponse> Service, IRequestContext<TRequest> Context)> GetRequestDispatchFilters<TRequest, TResponse>(
        this IServiceResolver serviceResolver, IRequestContext<TRequest> messageContext)
        => serviceResolver
            .GetServices<IRequestDispatchFilter<TRequest, TResponse>, TRequest, TResponse>(messageContext)
            .Select(t => (t.Item1, (IRequestContext<TRequest>)t.Item2));

    internal static IEnumerable<(IRequestHandlingFilter<TRequest, TResponse> Service, IRequestContext<TRequest> Context)> GetRequestHandlingFilters<TRequest, TResponse>(
        this IServiceResolver serviceResolver, IRequestContext<TRequest> requestContext, string key)
        => serviceResolver
            .GetServices<IRequestHandlingFilter<TRequest, TResponse>, TRequest, TResponse>(requestContext, ["global", key])
            .Select(t => (t.Item1, (IRequestContext<TRequest>)t.Item2));
    #endregion
    

    #region GetStreamHandlingFilters
    internal static IEnumerable<(IStreamDispatchFilter<TRequest, TResponse> Service, IStreamContext<TRequest> Context)> GetStreamDispatchFilters<TRequest, TResponse>(
        this IServiceResolver serviceResolver, IStreamContext<TRequest> streamContext)
        => serviceResolver
            .GetServices<IStreamDispatchFilter<TRequest, TResponse>, TRequest, TResponse>(streamContext)
            .Select(t => (t.Item1, (IStreamContext<TRequest>)t.Item2));
    internal static IEnumerable<(IStreamHandlingFilter<TRequest, TResponse> Service, IStreamContext<TRequest> Context)> GetStreamHandlingFilters<TRequest, TResponse>(
        this IServiceResolver serviceResolver, IStreamContext<TRequest> streamContext, string key)
        => serviceResolver
            .GetServices<IStreamHandlingFilter<TRequest, TResponse>, TRequest, TResponse>(streamContext, ["global", key])
            .Select(t => (t.Item1, (IStreamContext<TRequest>)t.Item2));
    #endregion
}
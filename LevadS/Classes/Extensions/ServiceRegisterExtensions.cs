using System.Diagnostics;
using System.Reflection;
using LevadS.Attributes;
using LevadS.Classes.Envelopers;
using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

internal static class ServiceRegisterExtensions
{
    internal static void RegisterServices(this IServiceRegister serviceRegister, Assembly assembly, Type genericInterfaceType, Action<IServiceRegister, string, Type, Type, Type?> registerAction)
    {
        var handlerTypes = assembly.FindImplementingTypes(genericInterfaceType);
        foreach (var handlerType in handlerTypes)
        {
            var scopeTypes = genericInterfaceType.IsAssignableTo(typeof(IFilter))
                ? handlerType
                    .GetCustomAttributes<BaseLevadSFilterForAttribute>(true)
                    .Select(a => a.FilteredHandlerType)
                    .ToArray()
                : genericInterfaceType.IsAssignableTo(typeof(IExceptionHandler))
                    ? handlerType
                        .GetCustomAttributes<BaseLevadSExceptionHandlerForAttribute>(true)
                        .Select(a => a.ProtectedHandlerType)
                        .ToArray()
                    : [];
            
            // regular registrations
            var attributes = handlerType.GetCustomAttributes<LevadSRegistrationAttribute>(true);
            foreach (var attribute in attributes)
            {
                if (handlerType is { IsGenericType: true, IsConstructedGenericType: false })
                {
                    throw new InvalidOperationException($"Handler type {(handlerType.FullName ?? handlerType.Name).ToReadableName()} is open generic, use LevadSGenericRegistration attribute to make it specific");
                }

                List<Type> interfaceTypes = [];
                if (attribute.InterfaceType != null)
                {
                    interfaceTypes.Add(attribute.InterfaceType);
                }
                else
                {
                    interfaceTypes.AddRange(handlerType.GetTypeInfo().ImplementedInterfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType));
                }

                foreach (var interfaceType in interfaceTypes)
                {
                    if (scopeTypes.Length != 0)
                    {
                        foreach (var scopeType in scopeTypes)
                        {
                            registerAction(serviceRegister, attribute.TopicPattern, interfaceType, handlerType, scopeType);
                        }
                    }
                    else
                    {
                        registerAction(serviceRegister, attribute.TopicPattern, interfaceType, handlerType, null);
                    }
                }
            }
            
            // generic registrations
            var genericAttributes = handlerType.GetCustomAttributes<BaseLevadSGenericRegistrationAttribute>(true);
            foreach (var attribute in genericAttributes)
            {
                if (handlerType.IsConstructedGenericType)
                {
                    throw new InvalidOperationException($"Handler type {(handlerType.FullName ?? handlerType.Name).ToReadableName()} is constructed generic, use LevadSRegistration attribute");
                }
                
                if (attribute.ServiceType!.GetGenericTypeDefinition() != handlerType)
                {
                    throw new InvalidOperationException($"Declared handler type {(attribute.ServiceType?.FullName ?? attribute.ServiceType?.Name ?? "<null>").ToReadableName()} does not match generic handler type {(handlerType.FullName ?? handlerType.Name).ToReadableName()}");
                }
                
                if (attribute.InterfaceType != null && !attribute.InterfaceType.IsAssignableFrom(attribute.ServiceType))
                {
                    throw new InvalidOperationException(
                        $"Declared handler interface {(attribute.InterfaceType?.FullName ?? attribute.InterfaceType?.Name ?? "<null>").ToReadableName()} does not match declared handler type {(attribute.ServiceType?.FullName ?? attribute.ServiceType?.Name ?? "<null>").ToReadableName()}");
                }

                List<Type> interfaceTypes = [];
                if (attribute.InterfaceType != null)
                {
                    interfaceTypes.Add(attribute.InterfaceType);
                }
                else
                {
                    interfaceTypes.AddRange(
                        (attribute.ServiceType ?? handlerType)
                            .GetTypeInfo()
                            .ImplementedInterfaces
                            .Where(i => 
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() == genericInterfaceType
                            )
                    );
                }

                foreach (var interfaceType in interfaceTypes)
                {
                    if (scopeTypes.Length != 0)
                    {
                        foreach (var scopeType in scopeTypes)
                        {
                            registerAction(serviceRegister, attribute.TopicPattern, interfaceType, attribute.ServiceType ?? handlerType, scopeType);
                        }
                    }
                    else
                    {
                        registerAction(serviceRegister, attribute.TopicPattern, interfaceType, attribute.ServiceType ?? handlerType, null);
                    }
                }
            }
        }
    }

    internal static IDisposable AddMessageHandler<TMessage, TMessageHandler>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IMessageContext<TMessage>, TMessageHandler>? factory = null, string? key = null)
        where TMessageHandler : class, IMessageHandler<TMessage>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TMessageHandler>(c);
        LogHandlerRegistration("Message Handler", typeof(TMessage), topicPattern);
        return serviceRegister.Register<IMessageHandler<TMessage>, TMessageHandler, TMessage>(new NoopEnveloper(), topicPattern, c => factory((IMessageContext<TMessage>)c), key: key);
    }
    
    internal static IDisposable AddRequestHandler<TRequest, TResponse, TRequestHandler>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IRequestContext<TRequest>, TRequestHandler>? factory = null, string? key = null)
        where TRequestHandler : class, IRequestHandler<TRequest, TResponse>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TRequestHandler>(c);
        LogHandlerRegistration("Request Handler", typeof(TRequest), topicPattern);
        return serviceRegister.Register<IRequestHandler<TRequest, TResponse>, TRequestHandler, TRequest, TResponse>(new NoopEnveloper(), topicPattern, c => factory((IRequestContext<TRequest>)c), key: key);
    }
    
    internal static IDisposable AddStreamHandler<TRequest, TResponse, TStreamHandler>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IStreamContext<TRequest>, TStreamHandler>? factory = null, string? key = null)
        where TStreamHandler : class, IStreamHandler<TRequest, TResponse>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TStreamHandler>(c);
        LogHandlerRegistration("Stream Handler", typeof(TRequest), topicPattern);
        return serviceRegister.Register<IStreamHandler<TRequest, TResponse>, TStreamHandler, TRequest, TResponse>(new NoopEnveloper(), topicPattern, c => factory((IStreamContext<TRequest>)c), key: key);
    }
    
    internal static IDisposable AddMessageHandlingFilter<TMessage, TMessageHandlingFilter>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IMessageContext<TMessage>, TMessageHandlingFilter>? factory = null, string? key = null)
        where TMessageHandlingFilter : class, IMessageHandlingFilter<TMessage>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TMessageHandlingFilter>(c);
        LogFilterRegistration("Message", "Handling", typeof(TMessage), topicPattern, key, typeof(TMessageHandlingFilter).GetInterfaces().Any(i => i == typeof(IExceptionHandler)));
        return serviceRegister.Register<IMessageHandlingFilter<TMessage>, TMessageHandlingFilter, TMessage>(new MessageHandlingEnveloper(), topicPattern, c => factory((IMessageContext<TMessage>)c), key: key);
    }
    
    internal static IDisposable AddRequestHandlingFilter<TRequest, TResponse, TRequestHandlingFilter>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IRequestContext<TRequest>, TRequestHandlingFilter>? factory = null, string? key = null)
        where TRequestHandlingFilter : class, IRequestHandlingFilter<TRequest, TResponse>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TRequestHandlingFilter>(c);
        LogFilterRegistration("Request", "Handling", typeof(TRequest), topicPattern, key, typeof(TRequestHandlingFilter).GetInterfaces().Any(i => i == typeof(IExceptionHandler)));
        return serviceRegister.Register<IRequestHandlingFilter<TRequest, TResponse>, TRequestHandlingFilter, TRequest, TResponse>(new RequestHandlingEnveloper(), topicPattern, c => factory((IRequestContext<TRequest>)c), key: key);
    }
    
    internal static IDisposable AddStreamHandlingFilter<TRequest, TResponse, TStreamHandlingFilter>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IStreamContext<TRequest>, TStreamHandlingFilter>? factory = null, string? key = null)
        where TStreamHandlingFilter : class, IStreamHandlingFilter<TRequest, TResponse>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TStreamHandlingFilter>(c);
        LogFilterRegistration("Stream", "Handling", typeof(TRequest), topicPattern, key, typeof(TStreamHandlingFilter).GetInterfaces().Any(i => i == typeof(IExceptionHandler)));
        return serviceRegister.Register<IStreamHandlingFilter<TRequest, TResponse>, TStreamHandlingFilter, TRequest, TResponse>(new StreamHandlingEnveloper(), topicPattern, c => factory((IStreamContext<TRequest>)c), key: key);
    }
    
    internal static IDisposable AddMessageExceptionHandler<TMessage, TException, TMessageExceptionHandler>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IMessageExceptionContext<TMessage, TException>, TMessageExceptionHandler>? factory = null, string? key = null)
        where TException : Exception
        where TMessageExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TMessageExceptionHandler>(c);
        LogFilterRegistration("Message", "Handling", typeof(TMessage), topicPattern, key, typeof(TMessageExceptionHandler).GetInterfaces().Any(i => i == typeof(IExceptionHandler)));
        return serviceRegister.Register<IMessageExceptionHandler<TMessage, TException>, TMessageExceptionHandler, TMessage>(new MessageHandlingEnveloper(), topicPattern, c => factory((IMessageExceptionContext<TMessage, TException>)c), key: key);
    }
    
    internal static IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TRequestExceptionHandler>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IRequestExceptionContext<TRequest, TException>, TRequestExceptionHandler>? factory = null, string? key = null)
        where TException : Exception
        where TRequestExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TRequestExceptionHandler>(c);
        LogFilterRegistration("Request", "Handling", typeof(TRequest), topicPattern, key, typeof(TRequestExceptionHandler).GetInterfaces().Any(i => i == typeof(IExceptionHandler)));
        return serviceRegister.Register<IRequestExceptionHandler<TRequest, TResponse, TException>, TRequestExceptionHandler, TRequest, TResponse>(new RequestHandlingEnveloper(), topicPattern, c => factory((IRequestExceptionContext<TRequest, TException>)c), key: key);
    }
    
    internal static IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TStreamExceptionHandler>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IStreamExceptionContext<TRequest, TException>, TStreamExceptionHandler>? factory = null, string? key = null)
        where TException : Exception
        where TStreamExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TStreamExceptionHandler>(c);
        LogFilterRegistration("Stream", "Handling", typeof(TRequest), topicPattern, key, typeof(TStreamExceptionHandler).GetInterfaces().Any(i => i == typeof(IExceptionHandler)));
        return serviceRegister.Register<IStreamExceptionHandler<TRequest, TResponse, TException>, TStreamExceptionHandler, TRequest, TResponse>(new StreamHandlingEnveloper(), topicPattern, c => factory((IStreamExceptionContext<TRequest, TException>)c), key: key);
    }
    
    internal static IDisposable AddMessageDispatchFilter<TMessage, TMessageDispatchFilter>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IMessageContext<TMessage>, TMessageDispatchFilter>? factory = null)
        where TMessageDispatchFilter : class, IMessageDispatchFilter<TMessage>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TMessageDispatchFilter>(c);
        LogFilterRegistration("Message", "Dispatch", typeof(TMessage), topicPattern);
        return serviceRegister.Register<IMessageDispatchFilter<TMessage>, TMessageDispatchFilter, TMessage>(new MessageDispatchEnveloper(), topicPattern, c => factory((IMessageContext<TMessage>)c));
    }
    
    internal static IDisposable AddRequestDispatchFilter<TRequest, TResponse, TRequestDispatchFilter>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IRequestContext<TRequest>, TRequestDispatchFilter>? factory = null)
        where TRequestDispatchFilter : class, IRequestDispatchFilter<TRequest, TResponse>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TRequestDispatchFilter>(c);
        LogFilterRegistration("Request", "Dispatch", typeof(TRequest), topicPattern);
        return serviceRegister.Register<IRequestDispatchFilter<TRequest, TResponse>, TRequestDispatchFilter, TRequest, TResponse>(new RequestDispatchEnveloper(), topicPattern, c => factory((IRequestContext<TRequest>)c));
    }
    
    internal static IDisposable AddStreamDispatchFilter<TRequest, TResponse, TStreamDispatchFilter>(this IServiceRegister serviceRegister, string topicPattern = "*", Func<IStreamContext<TRequest>, TStreamDispatchFilter>? factory = null)
        where TStreamDispatchFilter : class, IStreamDispatchFilter<TRequest, TResponse>
    {
        factory ??= c => c.ServiceProvider.CreateInstanceWithTopic<TStreamDispatchFilter>(c);
        LogFilterRegistration("Stream", "Dispatch", typeof(TRequest), topicPattern);
        return serviceRegister.Register<IStreamDispatchFilter<TRequest, TResponse>, TStreamDispatchFilter, TRequest, TResponse>(new StreamDispatchEnveloper(), topicPattern, c => factory((IStreamContext<TRequest>)c));
    }
    
    
    
    
    
    
    


    internal static IDisposable AddService(
        this IServiceRegister serviceRegister,
        Type serviceType,
        Type implementationType,
        Type inputType,
        Type? outputType,
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        string? key = null
    )
    {
        // LogHandlerRegistration("Message Handler", typeof(TMessage), topicPattern);
        return serviceRegister.Register(serviceType, implementationType, inputType, outputType ?? typeof(object), enveloper, topicPattern, key: key);
    }
    
    
    
    

    private static void LogHandlerRegistration(string kind, Type targetType, string topicPattern)
    {
        var fullName = targetType.FullName ?? targetType.Name;
        var readable = fullName.ToReadableName();
        Debug.WriteLine($"⦗-●)-⦘ Registered {kind} for '{readable}' with topic pattern '{topicPattern}'");
    }

    private static void LogFilterRegistration(string kind, string stage, Type targetType, string topicPattern, string? key = null, bool isExceptionHandler = false)
    {
        var service = isExceptionHandler
            ? "Exception Handler"
            : $"{stage} Filter";
        var fullName = targetType.FullName ?? targetType.Name;
        var readable = fullName.ToReadableName();
        var scopeText = key == "global"
            ? " global"
            : key != null
                ? " scoped"
                : "";
        Debug.WriteLine($"⦗-●)-⦘ Registered{scopeText} {kind} {service} for '{readable}' with topic pattern '{topicPattern}'");
    }
}
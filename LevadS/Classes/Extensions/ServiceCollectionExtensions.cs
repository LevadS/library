using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using LevadS.Attributes;
using LevadS.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

// internal static class ServiceCollectionExtensions
// {
//     public static IServiceCollection AllowResolvingKeyedServicesAsDictionary(
//         this IServiceCollection serviceCollection)
//         => serviceCollection
//             // KeyedServiceCache caches all the keys of a given type for a
//             // specific service type. By making it a singleton we only have
//             // determine the keys once, which makes resolving the dict very fast.
//             .AddSingleton(typeof(KeyedServiceCache<,>))
//             
//             // KeyedServiceCache depends on the IServiceCollection to get
//             // the list of keys. That's why we register that here as well, as it
//             // is not registered by default in MS.DI.
//             .AddSingleton(serviceCollection)
//             
//             // Last we make the registration for the dictionary itself, which maps
//             // to our custom type below. This registration must be  transient, as
//             // the containing services could have any lifetime and this registration
//             // should by itself not cause Captive Dependencies.
//             .AddTransient(typeof(IDictionary<,>), typeof(KeyedServiceDictionary<,>))
//             
//             // For completeness, let's also allow IReadOnlyDictionary to be resolved.
//             .AddTransient(typeof(IReadOnlyDictionary<,>), typeof(KeyedServiceDictionary<,>));
//
//     // We inherit from ReadOnlyDictionary, to disallow consumers from changing
//     // the wrapped dependencies while reusing all its functionality. This way
//     // we don't have to implement IDictionary<T,V> ourselves; too much work.
//     private sealed class KeyedServiceDictionary<TKey, TService>(
//         KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
//         : ReadOnlyDictionary<TKey, TService>(Create(keys, provider))
//         where TKey : notnull
//         where TService : notnull
//     {
//         private static Dictionary<TKey, TService> Create(
//             KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
//         {
//             var dict = new Dictionary<TKey, TService>(capacity: keys.Keys.Length);
//
//             foreach (var key in keys.Keys)
//             {
//                 dict[key] = provider.GetRequiredKeyedService<TService>(key);
//             }
//
//             return dict;
//         }
//     }
//
//     private sealed class KeyedServiceCache<TKey, TService>(IServiceCollection sc)
//         where TKey : notnull
//         where TService : notnull
//     {
//         // Once this class is resolved, all registrations are guaranteed to be
//         // made, so we can, at that point, safely iterate the collection to get
//         // the keys for the service type.
//         public TKey[] Keys { get; } = (
//             from service in sc
//             where service.ServiceKey != null
//             where service.ServiceKey!.GetType() == typeof(TKey)
//             where service.ServiceType == typeof(TService)
//             select (TKey)service.ServiceKey!)
//             .ToArray();
//     }
//     
//     public static ServiceCollection Clone(this IServiceCollection services)
//     {
//         var clone = new ServiceCollection();
//         foreach (var d in services) ((IServiceCollection)clone).Add(d);
//         return clone;
//     }
//     
//     #region AddTransientTopicMessageHandler
//     internal static IServiceCollection AddTransientTopicMessageHandler<TMessage, TImplementation>(
//         this IServiceCollection services, string topicPattern, string key, Func<TopicHandler, TImplementation> factory)
//         where TImplementation : class, IMessageHandler<TMessage>
//         => services
//             .AddTransient<ITopicMessageHandler<TMessage>, TopicMessageHandler<TMessage>>(p => 
//                 new TopicMessageHandler<TMessage>(p, factory, topicPattern, key)
//             )
//             .Also(_ => LogHandlerRegistration("Message Handler", typeof(TMessage), topicPattern));
//     
//     internal static IServiceCollection AddTransientTopicMessageHandler(this IServiceCollection services, string topicPattern, string key, Type messageType, Type implementationType)
//         => AddTransientHandlerCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicMessageHandler<>),
//             typeof(TopicMessageHandler<>),
//             [messageType],
//             implementationType
//         )
//         .Also(_ => LogHandlerRegistration("Message Handler", messageType, topicPattern));
//     #endregion
//     
//     #region AddTransientTopicRequestHandler
//     internal static IServiceCollection AddTransientTopicRequestHandler<TRequest, TResponse, TImplementation>(
//         this IServiceCollection services, string topicPattern, string key, Func<TopicHandler, TImplementation> factory)
//         where TImplementation : class, IRequestHandler<TRequest, TResponse>
//         => services
//             .AddTransient<ITopicRequestHandler<TRequest, TResponse>, TopicRequestHandler<TRequest, TResponse>>(p => 
//                 new TopicRequestHandler<TRequest, TResponse>(p, factory, topicPattern, key)
//             )
//             .Also(_ => LogHandlerRegistration("Request Handler", typeof(TRequest), topicPattern));
//
//     internal static void AddTransientTopicRequestHandler(this IServiceCollection services,
//         string topicPattern, string key, Type requestType, Type responseType, Type implementationType)
//     {
//         AddTransientHandlerCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicRequestHandler<,>),
//             typeof(TopicRequestHandler<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogHandlerRegistration("Request Handler", requestType, topicPattern));
//     }
//
//     #endregion
//     
//     #region AddTransientTopicStreamHandler
//     internal static IServiceCollection AddTransientTopicStreamHandler<TRequest, TResponse, TImplementation>(
//         this IServiceCollection services, string topicPattern, string key, Func<TopicHandler, TImplementation> factory)
//         where TImplementation : class, IStreamHandler<TRequest, TResponse>
//         => services
//             .AddTransient<ITopicStreamHandler<TRequest, TResponse>, TopicStreamHandler<TRequest, TResponse>>(p => 
//                 new TopicStreamHandler<TRequest, TResponse>(p, factory, topicPattern, key)
//             )
//             .Also(_ => LogHandlerRegistration("Stream Handler", typeof(TRequest), topicPattern));
//
//     internal static void AddTransientTopicStreamHandler(this IServiceCollection services, string topicPattern,
//         string key, Type requestType, Type responseType, Type implementationType)
//     {
//         AddTransientHandlerCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicStreamHandler<,>),
//             typeof(TopicStreamHandler<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogHandlerRegistration("Stream Handler", requestType, topicPattern));
//     }
//
//     #endregion
//
//     
//     #region AddKeyedTransientTopicMessageHandlingFilter
//     internal static void AddKeyedTransientTopicMessageHandlingFilter<TMessage, TImplementation>(
//         this IServiceCollection services, string topicPattern, string key, Func<TopicHandler, TImplementation> factory)
//         where TImplementation : class, IMessageHandlingFilter<TMessage>
//     => services.AddKeyedTransient<ITopicMessageHandlingFilter<TMessage>, TopicMessageHandlingFilter<TMessage>>(
//         key,
//         (p, k) => new TopicMessageHandlingFilter<TMessage>(p, factory, topicPattern, (string)k!)
//         )
//         .Also(_ => LogFilterRegistration("Message", typeof(TMessage), topicPattern, key, typeof(TImplementation).GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//
//     internal static void AddKeyedTransientTopicMessageHandlingFilter(
//         this IServiceCollection services, string topicPattern, string key, Type messageType, Type implementationType)
//     {
//         AddKeyedTransientHandlingFilterCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicMessageHandlingFilter<>),
//             typeof(TopicMessageHandlingFilter<>),
//             [messageType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Message", messageType, topicPattern, key, implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     }
//     
//     internal static void AddKeyedTransientTopicMessageExceptionHandler(
//         this IServiceCollection services, string topicPattern, string key, Type messageType, Type exceptionType, Type implementationType)
//     {
//         implementationType = typeof(MessageExceptionHandlerWrapper<,,>).MakeGenericType(messageType, exceptionType, implementationType);
//         AddKeyedTransientHandlingFilterCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicMessageHandlingFilter<>),
//             typeof(TopicMessageHandlingFilter<>),
//             [messageType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Message", messageType, topicPattern, key, implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     }
//
//     #endregion
//     
//     #region AddKeyedTransientTopicRequestHandlingFilter
//     internal static void AddKeyedTransientTopicRequestHandlingFilter<TRequest, TResponse, TImplementation>(
//         this IServiceCollection services, string topicPattern, string key, Func<TopicHandler, TImplementation> factory)
//         where TImplementation : class, IRequestHandlingFilter<TRequest, TResponse>
//         => services
//             .AddKeyedTransient<ITopicRequestHandlingFilter<TRequest, TResponse>, TopicRequestHandlingFilter<TRequest, TResponse>>(
//                 key,
//                 (p, k) => new TopicRequestHandlingFilter<TRequest, TResponse>(p, factory, topicPattern, (string)k!)
//             )
//             .Also(_ => LogFilterRegistration("Request", typeof(TRequest), topicPattern, key, typeof(TImplementation).GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     
//     internal static void AddKeyedTransientTopicRequestHandlingFilter(this IServiceCollection services, string topicPattern, string key, Type requestType, Type responseType, Type implementationType)
//     {
//         AddKeyedTransientHandlingFilterCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicRequestHandlingFilter<,>),
//             typeof(TopicRequestHandlingFilter<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Request", requestType, topicPattern, key, implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     }
//     
//     internal static void AddKeyedTransientTopicRequestExceptionHandler(this IServiceCollection services, string topicPattern, string key, Type requestType, Type responseType, Type exceptionType, Type implementationType)
//     {
//         implementationType = typeof(RequestExceptionHandlerWrapper<,,,>).MakeGenericType(requestType, responseType, exceptionType, implementationType);
//         AddKeyedTransientHandlingFilterCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicRequestHandlingFilter<,>),
//             typeof(TopicRequestHandlingFilter<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Request", requestType, topicPattern, key, implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     }
//     #endregion
//     
//     #region AddKeyedTransientTopicStreamHandlingFilter
//     internal static void AddKeyedTransientTopicStreamHandlingFilter<TRequest, TResponse, TImplementation>(
//         this IServiceCollection services, string topicPattern, string key, Func<TopicHandler, TImplementation> factory)
//         // where TRequest : IRequest<TResponse>
//         where TImplementation : class, IStreamHandlingFilter<TRequest, TResponse>
//         => services
//             .AddKeyedTransient<ITopicStreamHandlingFilter<TRequest, TResponse>, TopicStreamHandlingFilter<TRequest, TResponse>>(
//                 key,
//                 (p, k) => new TopicStreamHandlingFilter<TRequest, TResponse>(p, factory, topicPattern, (string)k!)
//             )
//             .Also(_ => LogFilterRegistration("Stream", typeof(TRequest), topicPattern, key, typeof(TImplementation).GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//
//     internal static void AddKeyedTransientTopicStreamHandlingFilter(this IServiceCollection services,
//         string topicPattern, string key, Type requestType, Type responseType, Type implementationType)
//     {
//         AddKeyedTransientHandlingFilterCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicStreamHandlingFilter<,>),
//             typeof(TopicStreamHandlingFilter<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Stream", requestType, topicPattern, key, implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     }
//     
//     internal static void AddKeyedTransientTopicStreamExceptionHandler(this IServiceCollection services,
//         string topicPattern, string key, Type requestType, Type responseType, Type exceptionType, Type implementationType)
//     {
//         implementationType = typeof(StreamExceptionHandlerWrapper<,,,>).MakeGenericType(requestType, responseType, exceptionType, implementationType);
//         AddKeyedTransientHandlingFilterCore(
//             services,
//             topicPattern,
//             key,
//             typeof(ITopicStreamHandlingFilter<,>),
//             typeof(TopicStreamHandlingFilter<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Stream", requestType, topicPattern, key, implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     }
//     #endregion
//     
//     internal static void AddSingletonTopicMessageDispatchFilter<TMessage, TImplementation>(
//         this IServiceCollection services, string topicPattern, Func<IServiceProvider, TImplementation> filterFactory)
//         where TImplementation : class, IMessageDispatchFilter<TMessage>
//         => services
//             .AddSingleton<ITopicMessageDispatchFilter<TMessage>, TopicMessageDispatchFilter<TMessage>>(
//                 p => new TopicMessageDispatchFilter<TMessage>(p, _ => filterFactory(p), topicPattern)
//             )
//             .Also(_ => LogFilterRegistration("Message", typeof(TMessage), topicPattern, "global", typeof(TImplementation).GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     
//     internal static void AddSingletonTopicMessageDispatchFilter(
//         this IServiceCollection services, string topicPattern, Type messageType, Type implementationType)
//         => AddSingletonDispatchFilterCore(
//             services,
//             topicPattern,
//             typeof(ITopicMessageDispatchFilter<>),
//             typeof(TopicMessageDispatchFilter<>),
//             [messageType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Message", messageType, topicPattern, "global", implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     
//     internal static void AddSingletonTopicRequestDispatchFilter<TRequest, TResponse, TImplementation>(
//         this IServiceCollection services, string topicPattern, Func<IServiceProvider, TImplementation> filterFactory)
//         where TImplementation : class, IRequestDispatchFilter<TRequest, TResponse>
//         => services
//             .AddSingleton<ITopicRequestDispatchFilter<TRequest, TResponse>, TopicRequestDispatchFilter<TRequest, TResponse>>(
//                 p => new TopicRequestDispatchFilter<TRequest, TResponse>(p, _ => filterFactory(p), topicPattern)
//             )
//             .Also(_ => LogFilterRegistration("Request", typeof(TRequest), topicPattern, "global", typeof(TImplementation).GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     
//     internal static void AddSingletonTopicRequestDispatchFilter(
//         this IServiceCollection services, string topicPattern, Type requestType, Type responseType, Type implementationType)
//         => AddSingletonDispatchFilterCore(
//             services,
//             topicPattern,
//             typeof(ITopicRequestDispatchFilter<,>),
//             typeof(TopicRequestDispatchFilter<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Request", requestType, topicPattern, "global", implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     
//     internal static void AddSingletonTopicStreamDispatchFilter<TRequest, TResponse, TImplementation>(
//         this IServiceCollection services, string topicPattern, Func<IServiceProvider, TImplementation> filterFactory)
//         where TImplementation : class, IStreamDispatchFilter<TRequest, TResponse>
//         => services
//             .AddSingleton<ITopicStreamDispatchFilter<TRequest, TResponse>, TopicStreamDispatchFilter<TRequest, TResponse>>(
//                 p => new TopicStreamDispatchFilter<TRequest, TResponse>(p, _ => filterFactory(p), topicPattern)
//             )
//             .Also(_ => LogFilterRegistration("Stream", typeof(TRequest), topicPattern, "global", typeof(TImplementation).GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//     
//     internal static void AddSingletonTopicStreamDispatchFilter(
//         this IServiceCollection services, string topicPattern, Type requestType, Type responseType, Type implementationType)
//         => AddSingletonDispatchFilterCore(
//             services,
//             topicPattern,
//             typeof(ITopicStreamDispatchFilter<,>),
//             typeof(TopicStreamDispatchFilter<,>),
//             [requestType, responseType],
//             implementationType
//         ).Also(_ => LogFilterRegistration("Stream", requestType, topicPattern, "global", implementationType.GetInterfaces().Any(i => i == typeof(IExceptionHandler))));
//
//     internal static void RegisterServices(this IServiceCollection services, Assembly assembly, Type genericInterfaceType, Action<IServiceCollection, string, Type, Type, Type?> registerAction)
//     {
//         var handlerTypes = assembly.FindImplementingTypes(genericInterfaceType);
//         foreach (var handlerType in handlerTypes)
//         {
//             var scopeTypes = genericInterfaceType.IsAssignableTo(typeof(IFilter))
//                 ? handlerType
//                     .GetCustomAttributes<BaseLevadSFilterForAttribute>(true)
//                     .Select(a => a.FilteredHandlerType)
//                     .ToArray()
//                 : genericInterfaceType.IsAssignableTo(typeof(IExceptionHandler))
//                     ? handlerType
//                         .GetCustomAttributes<BaseLevadSExceptionHandlerForAttribute>(true)
//                         .Select(a => a.ProtectedHandlerType)
//                         .ToArray()
//                     : [];
//             
//             // regular registrations
//             var attributes = handlerType.GetCustomAttributes<LevadSRegistrationAttribute>(true);
//             foreach (var attribute in attributes)
//             {
//                 if (handlerType is { IsGenericType: true, IsConstructedGenericType: false })
//                 {
//                     throw new InvalidOperationException($"Handler type {(handlerType.FullName ?? handlerType.Name).ToReadableName()} is open generic, use LevadSGenericRegistration attribute to make it specific");
//                 }
//
//                 List<Type> interfaceTypes = [];
//                 if (attribute.InterfaceType != null)
//                 {
//                     interfaceTypes.Add(attribute.InterfaceType);
//                 }
//                 else
//                 {
//                     interfaceTypes.AddRange(handlerType.GetTypeInfo().ImplementedInterfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType));
//                 }
//
//                 foreach (var interfaceType in interfaceTypes)
//                 {
//                     if (scopeTypes.Length != 0)
//                     {
//                         foreach (var scopeType in scopeTypes)
//                         {
//                             registerAction(services, attribute.TopicPattern, interfaceType, handlerType, scopeType);
//                         }
//                     }
//                     else
//                     {
//                         registerAction(services, attribute.TopicPattern, interfaceType, handlerType, null);
//                     }
//                 }
//             }
//             
//             // generic registrations
//             var genericAttributes = handlerType.GetCustomAttributes<BaseLevadSGenericRegistrationAttribute>(true);
//             foreach (var attribute in genericAttributes)
//             {
//                 if (handlerType.IsConstructedGenericType)
//                 {
//                     throw new InvalidOperationException($"Handler type {(handlerType.FullName ?? handlerType.Name).ToReadableName()} is constructed generic, use LevadSRegistration attribute");
//                 }
//                 
//                 if (attribute.ServiceType!.GetGenericTypeDefinition() != handlerType)
//                 {
//                     throw new InvalidOperationException($"Declared handler type {(attribute.ServiceType?.FullName ?? attribute.ServiceType?.Name ?? "<null>").ToReadableName()} does not match generic handler type {(handlerType.FullName ?? handlerType.Name).ToReadableName()}");
//                 }
//                 
//                 if (attribute.InterfaceType != null && !attribute.InterfaceType.IsAssignableFrom(attribute.ServiceType))
//                 {
//                     throw new InvalidOperationException(
//                         $"Declared handler interface {(attribute.InterfaceType?.FullName ?? attribute.InterfaceType?.Name ?? "<null>").ToReadableName()} does not match declared handler type {(attribute.ServiceType?.FullName ?? attribute.ServiceType?.Name ?? "<null>").ToReadableName()}");
//                 }
//
//                 List<Type> interfaceTypes = [];
//                 if (attribute.InterfaceType != null)
//                 {
//                     interfaceTypes.Add(attribute.InterfaceType);
//                 }
//                 else
//                 {
//                     interfaceTypes.AddRange(
//                         (attribute.ServiceType ?? handlerType)
//                             .GetTypeInfo()
//                             .ImplementedInterfaces
//                             .Where(i => 
//                                 i.IsGenericType &&
//                                 i.GetGenericTypeDefinition() == genericInterfaceType
//                             )
//                     );
//                 }
//
//                 foreach (var interfaceType in interfaceTypes)
//                 {
//                     if (scopeTypes.Length != 0)
//                     {
//                         foreach (var scopeType in scopeTypes)
//                         {
//                             registerAction(services, attribute.TopicPattern, interfaceType, attribute.ServiceType ?? handlerType, scopeType);
//                         }
//                     }
//                     else
//                     {
//                         registerAction(services, attribute.TopicPattern, interfaceType, attribute.ServiceType ?? handlerType, null);
//                     }
//                 }
//             }
//         }
//     }
//
//     private static void LogFilterRegistration(string kind, Type targetType, string topicPattern, string key, bool isExceptionHandler = false)
//     {
//         var service = isExceptionHandler
//             ? "Exception Handler"
//             : "Filter";
//         var fullName = targetType.FullName ?? targetType.Name;
//         var readable = fullName.ToReadableName();
//         var scopeText = key == "global" ? "global" : "scoped";
//         Debug.WriteLine($"⦗-●)-⦘ Registered {scopeText} {kind} {service} for '{readable}' with topic pattern '{topicPattern}'");
//     }
//
//     private static void LogHandlerRegistration(string kind, Type targetType, string topicPattern)
//     {
//         var fullName = targetType.FullName ?? targetType.Name;
//         var readable = fullName.ToReadableName();
//         Debug.WriteLine($"⦗-●)-⦘ Registered {kind} for '{readable}' with topic pattern '{topicPattern}'");
//     }
//
//     private static IServiceCollection AddSingletonDispatchFilterCore(
//         IServiceCollection services,
//         string topicPattern,
//         Type interfaceGenericDefinition,
//         Type wrapperGenericDefinition,
//         Type[] genericArgs,
//         Type implementationType)
//         => services
//             .AddSingleton(
//                 interfaceGenericDefinition.MakeGenericType(genericArgs),
//                 p => Activator.CreateInstance(
//                     wrapperGenericDefinition.MakeGenericType(genericArgs),
//                     p,
//                     (TopicHandler handler) => p.CreateInstanceWithTopic(implementationType, handler.Context!),
//                     topicPattern
//                 )!
//             );
//
//     private static IServiceCollection AddKeyedTransientHandlingFilterCore(
//         IServiceCollection services,
//         string topicPattern,
//         string key,
//         Type interfaceGenericDefinition,
//         Type wrapperGenericDefinition,
//         Type[] genericArgs,
//         Type implementationType)
//         => services.AddKeyedTransient(
//             interfaceGenericDefinition.MakeGenericType(genericArgs),
//             key,
//             (p, k) => Activator.CreateInstance(
//                 wrapperGenericDefinition.MakeGenericType(genericArgs),
//                 p,
//                 (TopicHandler handler) => p.CreateInstanceWithTopic(implementationType, handler.Context!),
//                 topicPattern,
//                 k
//             )!
//         );
//
//     private static IServiceCollection AddTransientHandlerCore(
//         IServiceCollection services,
//         string topicPattern,
//         string key,
//         Type interfaceGenericDefinition,
//         Type wrapperGenericDefinition,
//         Type[] genericArgs,
//         Type implementationType)
//         => services
//             .AddTransient(
//                 interfaceGenericDefinition.MakeGenericType(genericArgs),
//                 p => Activator.CreateInstance(
//                     wrapperGenericDefinition.MakeGenericType(genericArgs),
//                     p,
//                     (TopicHandler handler) => p.CreateInstanceWithTopic(implementationType, handler.Context!),
//                     topicPattern,
//                     key
//                 )!
//             );
//
//     private static IServiceCollection Also(this IServiceCollection services, Action<IServiceCollection> action)
//     {
//         action(services);
//         return services;
//     }
// }
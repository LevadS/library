using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LevadS.Classes;
using LevadS.Classes.Builders;
using LevadS.Classes.Extensions;
using LevadS.Extensions;
using LevadS.Interfaces;

namespace LevadS.Services;

internal class LevadSBuilder(IServiceCollection serviceCollection) : ILevadSBuilder
{
    protected IServiceCollection ServiceCollection => serviceCollection;

    public ILevadSBuilder EnableRuntimeRegistrations()
    {
        ServiceCollection.AddRuntimeRegistrationServices();

        return this;
    }

    public ILevadSBuilder RegisterServicesFromAssemblyContaining(Type type)
    {
        var assembly = Assembly.GetAssembly(type);
        if (assembly == null)
        {
            throw new NullReferenceException("The assembly is null.");
        }
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IMessageHandler<>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var key = $"{handlerType}";
                
                s.AddTransientTopicMessageHandler(
                    topicPattern,
                    key,
                    interfaceType.GenericTypeArguments.First(),
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IMessageDispatchFilter<>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddSingletonTopicMessageDispatchFilter(
                    topicPattern,
                    interfaceType.GenericTypeArguments.First(),
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IMessageExceptionHandler<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddKeyedTransientTopicMessageExceptionHandler(
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global",
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IMessageHandlingFilter<>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddKeyedTransientTopicMessageHandlingFilter(
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global",
                    interfaceType.GenericTypeArguments.First(),
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IRequestHandler<,>),
            (s, topicPattern, interfaceType, handlerType, _) =>
            {
                var key = $"{handlerType}";

                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddTransientTopicRequestHandler(
                    topicPattern,
                    key,
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IRequestDispatchFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddSingletonTopicRequestDispatchFilter(
                    topicPattern,
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IRequestHandlingFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddKeyedTransientTopicRequestHandlingFilter(
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global",
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IRequestExceptionHandler<,,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddKeyedTransientTopicRequestExceptionHandler(
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global",
                    genericArgs[0],
                    genericArgs[1],
                    genericArgs[2],
                    handlerType
                );
            });

        ServiceCollection.RegisterServices(
            assembly,
            typeof(IStreamHandler<,>),
            (s, topicPattern, interfaceType, handlerType, _) =>
            {
                var key = $"{handlerType}";

                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddTransientTopicStreamHandler(
                    topicPattern,
                    key,
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IStreamDispatchFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddSingletonTopicStreamDispatchFilter(
                    topicPattern,
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IStreamHandlingFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddKeyedTransientTopicStreamHandlingFilter(
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global",
                    genericArgs[0],
                    genericArgs[1],
                    handlerType
                );
            });
        
        ServiceCollection.RegisterServices(
            assembly,
            typeof(IStreamExceptionHandler<,,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                var genericArgs = interfaceType.GenericTypeArguments;
                s.AddKeyedTransientTopicStreamExceptionHandler(
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global",
                    genericArgs[0],
                    genericArgs[1],
                    genericArgs[2],
                    handlerType
                );
            });

        return this;
    }

    public IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IServiceProvider, IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = $"{typeof(THandler)}";
    
        var descriptors = ServiceCollection.AddTransientTopicMessageHandler<TMessage, THandler>(
            topicPattern,
            key,
            h => handlerFactory?.Invoke(
                h.ServiceProvider,
                (MessageContext<TMessage>)h.Context!
            ) ?? h.ServiceProvider.CreateInstanceWithTopic<THandler>(h.Context!)
        );
        
        return CreateMessageHandlerBuilder<TMessage>(key, descriptors);
    }

    protected virtual IMessageHandlerBuilder<TMessage> CreateMessageHandlerBuilder<TMessage>(string key, IEnumerable<ServiceDescriptor> serviceDescriptors)
        => new MessageHandlerBuilder<TMessage>(this, serviceCollection, key);

    public IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IServiceProvider, IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = $"{typeof(THandler)}";
    
        var descriptors = ServiceCollection.AddTransientTopicRequestHandler<TRequest, TResponse, THandler>(
            topicPattern,
            key,
            h => handlerFactory?.Invoke(
                h.ServiceProvider,
                (RequestContext<TRequest>)h.Context!
            ) ?? h.ServiceProvider.CreateInstanceWithTopic<THandler>(h.Context!)
        );
        
        return CreateRequestHandlerBuilder<TRequest, TResponse>(key, descriptors);
    }

    protected virtual IRequestHandlerBuilder<TRequest, TResponse> CreateRequestHandlerBuilder<TRequest, TResponse>(string key, IEnumerable<ServiceDescriptor> serviceDescriptors)
        => new RequestHandlerBuilder<TRequest, TResponse>(this, serviceCollection, key);

    public IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IServiceProvider, IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IStreamHandler<TRequest, TResponse>
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = $"{typeof(THandler)}";

        var descriptors = ServiceCollection.AddTransientTopicStreamHandler<TRequest, TResponse, THandler>(
            topicPattern,
            key,
            h => handlerFactory?.Invoke(
                h.ServiceProvider,
                (StreamContext<TRequest>)h.Context!
            ) ?? h.ServiceProvider.CreateInstanceWithTopic<THandler>(h.Context!)
        );
        
        return CreateStreamHandlerBuilder<TRequest, TResponse>(key, descriptors);
    }

    protected virtual IStreamHandlerBuilder<TRequest, TResponse> CreateStreamHandlerBuilder<TRequest, TResponse>(string key, IEnumerable<ServiceDescriptor> serviceDescriptors)
        => new StreamHandlerBuilder<TRequest, TResponse>(this, serviceCollection, key);

    public ILevadSBuilder AddMessageFilter<TMessage, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IMessageHandlingFilter<TMessage>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        serviceCollection.AddKeyedTransientTopicMessageHandlingFilter<TMessage, TFilter>(topicPattern, "global", h => filterFactory(h.ServiceProvider));

        return this;
    }

    public ILevadSBuilder AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);
        
        serviceCollection.AddKeyedTransientTopicRequestHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, "global", h => filterFactory(h.ServiceProvider));

        return this;
    }

    public ILevadSBuilder AddRequestFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IRequestHandlingFilter<TRequest>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);
        
        serviceCollection.AddKeyedTransientTopicRequestHandlingFilter<TRequest, object, TFilter>(topicPattern, "global", h => filterFactory(h.ServiceProvider));

        return this;
    }

    public ILevadSBuilder AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)  where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        serviceCollection.AddKeyedTransientTopicStreamHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, "global", h => filterFactory(h.ServiceProvider));

        return this;
    }

    public ILevadSBuilder AddStreamFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IStreamHandlingFilter<TRequest>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        serviceCollection.AddKeyedTransientTopicStreamHandlingFilter<TRequest, object, TFilter>(topicPattern, "global", h => filterFactory(h.ServiceProvider));

        return this;
    }

    public ILevadSBuilder AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);
        
        ServiceCollection.AddSingletonTopicMessageDispatchFilter<TMessage, TFilter>(topicPattern, filterFactory);

        return this;
    }

    public ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        ServiceCollection.AddSingletonTopicRequestDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

        return this;
    }

    public ILevadSBuilder AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);
        
        serviceCollection.AddSingletonTopicRequestDispatchFilter<TRequest, object, TFilter>(topicPattern, filterFactory);

        return this;
    }

    public ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        ServiceCollection.AddSingletonTopicStreamDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);
        
        return this;
    }

    public ILevadSBuilder AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory = null) where TFilter : class, IStreamDispatchFilter<TRequest>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);
        
        serviceCollection.AddSingletonTopicStreamDispatchFilter<TRequest, object, TFilter>(topicPattern, filterFactory);

        return this;
    }

    public ILevadSBuilder AddLevada<TLevada>(string levadaName, Action<ILevadaBuilder<TLevada>> builderAction)
        where TLevada : ILevada
    {
        ServiceCollection.AddKeyedSingleton<ILevadaService>(levadaName, (serviceProvider, _) =>
        {
            var levada = ActivatorUtilities.CreateInstance<TLevada>(serviceProvider);
            var builder = new LevadaBuilder<TLevada>();
            builderAction(builder);
            
            return new LevadaWrapper<TLevada>(serviceProvider, levada, builder);
        });
        
        return this;
    }

    public ILevadSBuilder WarmUpMessageHandling<TMessage>()
    {
        ServiceProviderExtensions.GetVariantMessageServiceTypes<TMessage, ITopicMessageDispatchFilter<TMessage>>();
        ServiceProviderExtensions.GetVariantMessageServiceTypes<TMessage, ITopicMessageHandler<TMessage>>();
        ServiceProviderExtensions.GetVariantMessageServiceTypes<TMessage, ITopicMessageHandlingFilter<TMessage>>();
        
        return this;
    }

    public ILevadSBuilder WarmUpRequestHandling<TRequest, TResponse>() 
    {
        ServiceProviderExtensions.GetVariantRequestServiceTypes<TRequest, TResponse, ITopicRequestDispatchFilter<TRequest, TResponse>>();
        ServiceProviderExtensions.GetVariantRequestServiceTypes<TRequest, TResponse, ITopicRequestHandler<TRequest, TResponse>>();
        ServiceProviderExtensions.GetVariantRequestServiceTypes<TRequest, TResponse, ITopicRequestHandlingFilter<TRequest, TResponse>>();

        return this;
    }

    public ILevadSBuilder WarmUpStreamHandling<TRequest, TResponse>() 
    {
        ServiceProviderExtensions.GetVariantRequestServiceTypes<TRequest, TResponse, ITopicStreamDispatchFilter<TRequest, TResponse>>();
        ServiceProviderExtensions.GetVariantRequestServiceTypes<TRequest, TResponse, ITopicStreamHandler<TRequest, TResponse>>();
        ServiceProviderExtensions.GetVariantRequestServiceTypes<TRequest, TResponse, ITopicStreamHandlingFilter<TRequest, TResponse>>();
        
        return this;
    }
}
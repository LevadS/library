using System.Reflection;
using LevadS.Classes;
using LevadS.Classes.Envelopers;
using LevadS.Classes.Extensions;
using LevadS.Delegates;
using LevadS.Extensions;
using LevadS.Interfaces;

namespace LevadS.Services;


internal class LevadSBuilder(IServiceRegister serviceRegister) :
    ILevadSBuilder,
    IHandlersRegister,
    IFiltersRegister,
    IDispatchFiltersRegister,
    IExceptionHandlersRegister,
    IMessageServicesRegister,
    IRequestServicesRegister,
    IStreamServicesRegister
{
    public ILevadSBuilder RegisterServicesFromAssemblyContaining(Type type)
    {
        var assembly = Assembly.GetAssembly(type);
        if (assembly == null)
        {
            throw new NullReferenceException("The assembly is null.");
        }
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IMessageHandler<>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    null,
                    new NoopEnveloper(),
                    topicPattern,
                    $"{handlerType}"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IMessageDispatchFilter<>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    null,
                    new MessageDispatchEnveloper(),
                    topicPattern
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IMessageExceptionHandler<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    null,
                    new MessageHandlingEnveloper(),
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IMessageHandlingFilter<>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    null,
                    new MessageHandlingEnveloper(),
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IRequestHandler<,>),
            (s, topicPattern, interfaceType, handlerType, _) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new NoopEnveloper(),
                    topicPattern,
                    $"{handlerType}"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IRequestDispatchFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new RequestDispatchEnveloper(),
                    topicPattern
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IRequestHandlingFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new RequestHandlingEnveloper(),
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IRequestExceptionHandler<,,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new RequestHandlingEnveloper(),
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IStreamHandler<,>),
            (s, topicPattern, interfaceType, handlerType, _) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new NoopEnveloper(),
                    topicPattern,
                    $"{handlerType}"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IStreamDispatchFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new StreamDispatchEnveloper(),
                    topicPattern
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IStreamHandlingFilter<,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new StreamHandlingEnveloper(),
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global"
                );
            });
        
        serviceRegister.RegisterServices(
            assembly,
            typeof(IStreamExceptionHandler<,,>),
            (s, topicPattern, interfaceType, handlerType, scopeType) =>
            {
                s.AddService(
                    interfaceType,
                    handlerType,
                    interfaceType.GenericTypeArguments[0],
                    interfaceType.GenericTypeArguments[1],
                    new StreamHandlingEnveloper(),
                    topicPattern,
                    scopeType != null
                        ? $"{scopeType}"
                        : "global"
                );
            });

        return this;
    }

    public IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = Guid.NewGuid().ToString();
    
        return new MessageHandlerBuilder<TMessage>(this, serviceRegister, key, serviceRegister.AddMessageHandler<TMessage, THandler>(topicPattern, handlerFactory, key));
    }

    public IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = Guid.NewGuid().ToString();
        
        return new RequestHandlerBuilder<TRequest, TResponse>(this, serviceRegister, key, serviceRegister.AddRequestHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory, key));
    }
    
    public IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IStreamHandler<TRequest, TResponse>
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = Guid.NewGuid().ToString();
        
        return new StreamHandlerBuilder<TRequest, TResponse>(this, serviceRegister, key, serviceRegister.AddStreamHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory, key));
    }

    public ILevadSBuilder AddMessageFilter<TMessage, TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory)
        where TFilter : class, IMessageHandlingFilter<TMessage>
    {
        serviceRegister.AddMessageHandlingFilter<TMessage, TFilter>(topicPattern, filterFactory, "global");

        return this;
    }

    public ILevadSBuilder AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
    {
        serviceRegister.AddRequestHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory, "global");

        return this;
    }

    IDisposable IRequestFiltersRegister.AddRequestFilter<TRequest, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        =>  serviceRegister.AddRequestHandlingFilter<TRequest, object, TFilter>(topicPattern, filterFactory, "global");

    IDisposable IRequestFiltersRegister.AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        =>  serviceRegister.AddRequestHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory, "global");

    public ILevadSBuilder AddRequestFilter<TRequest, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        where TFilter : class, IRequestHandlingFilter<TRequest>
    {
        serviceRegister.AddRequestHandlingFilter<TRequest, object, TFilter>(topicPattern, filterFactory, "global");

        return this;
    }

    public ILevadSBuilder AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
    {
        serviceRegister.AddStreamHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory, "global");

        return this;
    }

    IDisposable IStreamFiltersRegister.AddStreamFilter<TRequest, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => serviceRegister.AddStreamHandlingFilter<TRequest, object, TFilter>(topicPattern, filterFactory, "global");

    IDisposable IStreamFiltersRegister.AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => serviceRegister.AddStreamHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory, "global");

    public ILevadSBuilder AddStreamFilter<TRequest, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        where TFilter : class, IStreamHandlingFilter<TRequest>
    {
        serviceRegister.AddStreamHandlingFilter<TRequest, object, TFilter>(topicPattern, filterFactory, "global");

        return this;
    }

    public ILevadSBuilder AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>
    {
        serviceRegister.AddMessageDispatchFilter<TMessage, TFilter>(topicPattern, filterFactory);

        return this;
    }

    public ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>
    {
        serviceRegister.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

        return this;
    }

    IDisposable IRequestDispatchFiltersRegister.AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        => serviceRegister.AddRequestDispatchFilter<TRequest, object, TFilter>(topicPattern, filterFactory);

    IDisposable IRequestDispatchFiltersRegister.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        => serviceRegister.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

    public ILevadSBuilder AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>
    {
        serviceRegister.AddRequestDispatchFilter<TRequest, object, TFilter>(topicPattern, filterFactory);

        return this;
    }

    public ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>
    {
        serviceRegister.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);
        
        return this;
    }

    IDisposable IStreamDispatchFiltersRegister.AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => serviceRegister.AddStreamDispatchFilter<TRequest, object, TFilter>(topicPattern, filterFactory);

    IDisposable IStreamDispatchFiltersRegister.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => serviceRegister.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

    public ILevadSBuilder AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>
    {
        serviceRegister.AddStreamDispatchFilter<TRequest, object, TFilter>(topicPattern, filterFactory);

        return this;
    }

    // public ILevadSBuilder AddLevada<TLevada>(string levadaName, Action<ILevadaBuilder<TLevada>> builderAction)
    //     where TLevada : ILevada
    // {
    //     serviceRegister.AddKeyedSingleton<ILevadaService>(levadaName, (serviceProvider, _) =>
    //     {
    //         var levada = ActivatorUtilities.CreateInstance<TLevada>(serviceProvider);
    //         var builder = new LevadaBuilder<TLevada>();
    //         builderAction(builder);
    //         
    //         return new LevadaWrapper<TLevada>(serviceProvider, levada, builder);
    //     });
    //     
    //     return this;
    // }

    IDisposableMessageHandlerBuilder<TMessage> IMessageHandlersRegister.AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IMessageContext<TMessage>, THandler>? handlerFactory)
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = Guid.NewGuid().ToString();
    
        return new MessageHandlerBuilder<TMessage>(this, serviceRegister, key, registrationAction: () => serviceRegister.AddMessageHandler<TMessage, THandler>(topicPattern, handlerFactory, key));
    }

    IDisposableRequestHandlerBuilder<TRequest, TResponse> IRequestHandlersRegister.AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IRequestContext<TRequest>, THandler>? handlerFactory)
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = Guid.NewGuid().ToString();
        
        return new RequestHandlerBuilder<TRequest, TResponse>(this, serviceRegister, key, registrationAction: () => serviceRegister.AddRequestHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory, key));
    }

    IDisposableStreamHandlerBuilder<TRequest, TResponse> IStreamHandlersRegister.AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IStreamContext<TRequest>, THandler>? handlerFactory)
    {
        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var key = Guid.NewGuid().ToString();
        
        return new StreamHandlerBuilder<TRequest, TResponse>(this, serviceRegister, key, registrationAction: () => serviceRegister.AddStreamHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory, key));
    }

    IDisposable IMessageFiltersRegister.AddMessageFilter<TMessage, TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory)
        => serviceRegister.AddMessageHandlingFilter<TMessage, TFilter>(topicPattern, filterFactory, "global");

    IDisposable IMessageDispatchFiltersRegister.AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern,
        Func<IMessageContext<TMessage>, TFilter>? filterFactory)
        => serviceRegister.AddMessageDispatchFilter(topicPattern, filterFactory);

    public IDisposable AddMessageExceptionHandler<TMessage, TException>(string topicPattern,
        MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate) where TException : Exception
    {
        throw new NotImplementedException();
    }

    public IDisposable AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null) where TException : Exception where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
    {
        throw new NotImplementedException();
    }

    public IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
        RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate) where TException : Exception
    {
        throw new NotImplementedException();
    }

    public IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) where TException : Exception where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
    {
        throw new NotImplementedException();
    }

    public IDisposable AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) where TException : Exception where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
    {
        throw new NotImplementedException();
    }

    public IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
        StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate) where TException : Exception
    {
        throw new NotImplementedException();
    }

    public IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) where TException : Exception where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
    {
        throw new NotImplementedException();
    }

    public IDisposable AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) where TException : Exception where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
    {
        throw new NotImplementedException();
    }
}
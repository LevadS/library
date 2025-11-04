using System.Reflection;
using LevadS.Classes;
using LevadS.Classes.Envelopers;
using LevadS.Classes.Extensions;
using LevadS.Delegates;
using LevadS.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes.Builders;

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
        
        serviceRegister.RegisterServices(assembly);

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
        MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate) 
        where TException : Exception
        => ((IMessageFiltersRegister)this).AddMessageFilter<TMessage, MessageExceptionHandlerDelegateWrapper<TMessage, TException>>(topicPattern, p => new MessageExceptionHandlerDelegateWrapper<TMessage, TException>(exceptionHandlerDelegate));

    public IDisposable AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null) 
        where TException : Exception 
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => ((IMessageFiltersRegister)this).AddMessageFilter<TMessage, MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>>(
            topicPattern,
            p => new MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    public IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
        RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate) 
        where TException : Exception
        => ((IRequestFiltersRegister)this).AddRequestFilter<TRequest, TResponse, RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));

    public IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) 
        where TException : Exception 
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => ((IRequestFiltersRegister)this).AddRequestFilter<TRequest, TResponse, RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(
            topicPattern,
            p => new RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    public IDisposable AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) 
        where TException : Exception 
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => ((IRequestFiltersRegister)this).AddRequestFilter<TRequest, RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>>(
            topicPattern,
            p => new RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    public IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
        StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate) 
        where TException : Exception
        => ((IStreamFiltersRegister)this).AddStreamFilter<TRequest, TResponse, StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));

    public IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) 
        where TException : Exception 
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => ((IStreamFiltersRegister)this).AddStreamFilter<TRequest, TResponse, StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(
            topicPattern,
            p => new StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    public IDisposable AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null) 
        where TException : Exception 
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => ((IStreamFiltersRegister)this).AddStreamFilter<TRequest, StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>>(
            topicPattern,
            p => new StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(exceptionHandlerFactory)
        );
}
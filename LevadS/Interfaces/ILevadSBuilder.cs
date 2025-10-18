using LevadS.Classes;
using LevadS.Delegates;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

public interface ILevadSBuilder
{
    ILevadSBuilder EnableRuntimeRegistrations();
    
    ILevadSBuilder RegisterServicesFromAssemblyContaining(Type type);

    ILevadSBuilder RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssemblyContaining(typeof(T));

    #region AddMessageHandler
    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage>(string topicPattern, Delegate handler)
        => AddMessageHandler<TMessage, MessageHandlerDelegateWrapper<TMessage>>(topicPattern, (serviceProvider, _) => new MessageHandlerDelegateWrapper<TMessage>(serviceProvider, handler));
    
    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage>(Delegate handler)
        => AddMessageHandler<TMessage>("*", handler);
    
    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(string topicPattern,
        Func<IServiceProvider, IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>;

    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(
        Func<IServiceProvider, IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>
        => AddMessageHandler<TMessage, THandler>("*", handlerFactory);
    #endregion
    
    #region AddRequestHandler
    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
        where TRequest : IRequest<TResponse>
        => AddRequestHandler<TRequest, TResponse, RequestHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern, (serviceProvider, _) => new RequestHandlerDelegateWrapper<TRequest, TResponse>(serviceProvider, handler));
    
    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(Delegate handler)
        where TRequest : IRequest<TResponse>
        => AddRequestHandler<TRequest, TResponse>("*", handler);
    
    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IServiceProvider, IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>;

    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(
        Func<IServiceProvider, IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
        => AddRequestHandler<TRequest, TResponse, THandler>("*", handlerFactory);
    #endregion
    
    #region AddStreamHandler
    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
        where TRequest : IRequest<TResponse>
        => AddStreamHandler<TRequest, TResponse, StreamHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern, (serviceProvider, _) => new StreamHandlerDelegateWrapper<TRequest, TResponse>(serviceProvider, handler));
    
    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse>(Delegate handler)
        where TRequest : IRequest<TResponse>
        => AddStreamHandler<TRequest, TResponse>("*", handler);
    
    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IServiceProvider, IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IStreamHandler<TRequest, TResponse>;

    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(
        Func<IServiceProvider, IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IStreamHandler<TRequest, TResponse>
        => AddStreamHandler<TRequest, TResponse, THandler>("*", handlerFactory);
    #endregion
    
    #region AddMessageFilter
    ILevadSBuilder AddMessageFilter<TMessage>(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageFilter<TMessage>(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageFilter<TMessage, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    ILevadSBuilder AddMessageFilter<TMessage, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => AddMessageFilter<TMessage, TFilter>("*", filterFactory);
    #endregion
    
    #region AddRequestFilter
    ILevadSBuilder AddRequestFilter<TRequest, TResponse>(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestFilter<TRequest, TResponse>(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    ILevadSBuilder AddRequestFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => AddRequestFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddRequestFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>;

    ILevadSBuilder AddRequestFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>
        => AddRequestFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddStreamFilter
    ILevadSBuilder AddStreamFilter<TRequest, TResponse>(string topicPattern, StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamFilter<TRequest, TResponse>(StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>("*", p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>;

    ILevadSBuilder AddStreamFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
        => AddStreamFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddStreamFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>;

    ILevadSBuilder AddStreamFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>
        => AddStreamFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddMessageDispatchFilter
    ILevadSBuilder AddMessageDispatchFilter<TMessage>(string topicPattern, MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>(topicPattern, p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageDispatchFilter<TMessage>(MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>("*", p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>;

    ILevadSBuilder AddMessageDispatchFilter<TMessage, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>
        => AddMessageDispatchFilter<TMessage, TFilter>("*", filterFactory);
    #endregion
    
    #region AddRequestDispatchFilter
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse>(string topicPattern, RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse>(RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>("*", p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>;

    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>;

    ILevadSBuilder AddRequestDispatchFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>
        => AddRequestDispatchFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddStreamDispatchFilter
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse>(string topicPattern, StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse>(StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>("*", p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>;

    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>
        => AddStreamDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>;

    ILevadSBuilder AddStreamDispatchFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>
        => AddStreamDispatchFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddMessageExceptionHandler
    ILevadSBuilder AddMessageExceptionHandler<TMessage, TException>(string topicPattern, MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddMessageFilter<TMessage, MessageExceptionHandlerDelegateWrapper<TMessage, TException>>(topicPattern, p => new MessageExceptionHandlerDelegateWrapper<TMessage, TException>(exceptionHandlerDelegate));
    
    ILevadSBuilder AddMessageExceptionHandler<TMessage, TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddMessageExceptionHandler("*", exceptionHandlerDelegate);

    ILevadSBuilder AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => AddMessageFilter<TMessage, MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>>(
            topicPattern,
            p => new MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(p, exceptionHandlerFactory)
        );

    ILevadSBuilder AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    #endregion
    
    #region AddRequestExceptionHandler
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        where TRequest : IRequest<TResponse>
        => AddRequestFilter<TRequest, TResponse, RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        where TRequest : IRequest<TResponse>
        => AddRequestExceptionHandler("*", exceptionHandlerDelegate);
    
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TRequest : IRequest<TResponse>
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => AddRequestFilter<TRequest, TResponse, RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(
            topicPattern,
            p => new RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(p, exceptionHandlerFactory)
        );

    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TRequest : IRequest<TResponse>
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => AddRequestFilter<TRequest, RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>>(
            topicPattern,
            p => new RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(p, exceptionHandlerFactory)
        );

    ILevadSBuilder AddRequestExceptionHandler<TRequest, TExceptionHandler, TException>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    #endregion
    
    #region AddStreamExceptionHandler
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern, StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        where TRequest : IRequest<TResponse>
        => AddStreamFilter<TRequest, TResponse, StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        where TRequest : IRequest<TResponse>
        => AddStreamExceptionHandler("*", exceptionHandlerDelegate);
    
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TRequest : IRequest<TResponse>
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => AddStreamFilter<TRequest, TResponse, StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(
            topicPattern,
            p => new StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(p, exceptionHandlerFactory)
        );

    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TRequest : IRequest<TResponse>
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => AddStreamFilter<TRequest, StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>>(
            topicPattern,
            p => new StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(p, exceptionHandlerFactory)
        );

    ILevadSBuilder AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    #endregion

    internal ILevadSBuilder AddLevada<TLevada>(string levadaName, Action<ILevadaBuilder<TLevada>> builder)
        where TLevada : ILevada;

    ILevadSBuilder WarmUpMessageHandling<TMessage>();

    ILevadSBuilder WarmUpRequestHandling<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>;

    ILevadSBuilder WarmUpStreamHandling<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>;
}
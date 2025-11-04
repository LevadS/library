using LevadS.Classes;
using LevadS.Delegates;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

public interface ILevadSBuilder
{
    ILevadSBuilder RegisterServicesFromAssemblyContaining(Type type);

    ILevadSBuilder RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssemblyContaining(typeof(T));

    #region AddMessageHandler
    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage>(string topicPattern, Delegate handler)
        => AddMessageHandler<TMessage, MessageHandlerDelegateWrapper<TMessage>>(topicPattern, _ => new MessageHandlerDelegateWrapper<TMessage>(handler));
    
    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage>(Delegate handler)
        => AddMessageHandler<TMessage>("*", handler);
    
    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(string topicPattern,
        Func<IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>;

    IMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(
        Func<IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>
        => AddMessageHandler<TMessage, THandler>("*", handlerFactory);
    #endregion
    
    #region AddRequestHandler
    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
        => AddRequestHandler<TRequest, TResponse, RequestHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern, _ => new RequestHandlerDelegateWrapper<TRequest, TResponse>(handler));
    
    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(Delegate handler)
        => AddRequestHandler<TRequest, TResponse>("*", handler);
    
    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IRequestHandler<TRequest, TResponse>;

    IRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(
        Func<IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IRequestHandler<TRequest, TResponse>
        => AddRequestHandler<TRequest, TResponse, THandler>("*", handlerFactory);
    #endregion
    
    #region AddStreamHandler
    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
        => AddStreamHandler<TRequest, TResponse, StreamHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern, _ => new StreamHandlerDelegateWrapper<TRequest, TResponse>(handler));
    
    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse>(Delegate handler)
        => AddStreamHandler<TRequest, TResponse>("*", handler);
    
    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IStreamHandler<TRequest, TResponse>;

    IStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(
        Func<IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IStreamHandler<TRequest, TResponse>
        => AddStreamHandler<TRequest, TResponse, THandler>("*", handlerFactory);
    #endregion
    
    #region AddMessageFilter
    ILevadSBuilder AddMessageFilter<TMessage>(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageFilter<TMessage>(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageFilter<TMessage, TFilter>(string topicPattern,
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    ILevadSBuilder AddMessageFilter<TMessage, TFilter>(
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => AddMessageFilter<TMessage, TFilter>("*", filterFactory);
    #endregion
    
    #region AddRequestFilter
    ILevadSBuilder AddRequestFilter<TRequest, TResponse>(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestFilter<TRequest, TResponse>(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    ILevadSBuilder AddRequestFilter<TRequest, TResponse, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => AddRequestFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddRequestFilter<TRequest, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>;

    ILevadSBuilder AddRequestFilter<TRequest, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>
        => AddRequestFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddStreamFilter
    ILevadSBuilder AddStreamFilter<TRequest, TResponse>(string topicPattern, StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamFilter<TRequest, TResponse>(StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>("*", p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>;

    ILevadSBuilder AddStreamFilter<TRequest, TResponse, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
        => AddStreamFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddStreamFilter<TRequest, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>;

    ILevadSBuilder AddStreamFilter<TRequest, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>
        => AddStreamFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddMessageDispatchFilter
    ILevadSBuilder AddMessageDispatchFilter<TMessage>(string topicPattern, MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>(topicPattern, p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageDispatchFilter<TMessage>(MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>("*", p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));
    
    ILevadSBuilder AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern,
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>;

    ILevadSBuilder AddMessageDispatchFilter<TMessage, TFilter>(
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>
        => AddMessageDispatchFilter<TMessage, TFilter>("*", filterFactory);
    #endregion
    
    #region AddRequestDispatchFilter
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse>(string topicPattern, RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse>(RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>("*", p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>;

    ILevadSBuilder AddRequestDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>;

    ILevadSBuilder AddRequestDispatchFilter<TRequest, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>
        => AddRequestDispatchFilter<TRequest, TFilter>("*", filterFactory);
    #endregion
    
    #region AddStreamDispatchFilter
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse>(string topicPattern, StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse>(StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>("*", p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>;

    ILevadSBuilder AddStreamDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>
        => AddStreamDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);
    
    ILevadSBuilder AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>;

    ILevadSBuilder AddStreamDispatchFilter<TRequest, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
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
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => AddMessageFilter<TMessage, MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>>(
            topicPattern,
            p => new MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    ILevadSBuilder AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    #endregion
    
    #region AddRequestExceptionHandler
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddRequestFilter<TRequest, TResponse, RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddRequestExceptionHandler("*", exceptionHandlerDelegate);
    
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => AddRequestFilter<TRequest, TResponse, RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(
            topicPattern,
            p => new RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    ILevadSBuilder AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    ILevadSBuilder AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => AddRequestFilter<TRequest, RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>>(
            topicPattern,
            p => new RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    ILevadSBuilder AddRequestExceptionHandler<TRequest, TExceptionHandler, TException>(
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    #endregion
    
    #region AddStreamExceptionHandler
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern, StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddStreamFilter<TRequest, TResponse, StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddStreamExceptionHandler("*", exceptionHandlerDelegate);
    
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => AddStreamFilter<TRequest, TResponse, StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(
            topicPattern,
            p => new StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    ILevadSBuilder AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    ILevadSBuilder AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => AddStreamFilter<TRequest, StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>>(
            topicPattern,
            p => new StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(exceptionHandlerFactory)
        );

    ILevadSBuilder AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    #endregion
}
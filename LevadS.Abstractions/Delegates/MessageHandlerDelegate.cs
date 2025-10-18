using LevadS.Interfaces;

namespace LevadS.Delegates;

public delegate Task MessageDispatchFilterNextDelegate(string? topic = null, Dictionary<string, object>? headers = null);

public delegate Task MessageDispatchFilterDelegate<in TMessage>(IMessageContext<TMessage> requestContext, MessageDispatchFilterNextDelegate next);



public delegate Task MessageHandlingFilterNextDelegate();

public delegate Task MessageHandlingFilterDelegate<in TMessage>(IMessageContext<TMessage> requestContext, MessageHandlingFilterNextDelegate next);



public delegate Task<bool> MessageExceptionHandlerDelegate<in TMessage, in TException>(IMessageExceptionContext<TMessage, TException> exceptionContext)
    where TException : Exception;



public delegate Task<TResponse> RequestDispatchFilterNextDelegate<TResponse>(string? topic = null, Dictionary<string, object>? headers = null);

public delegate Task<TResponse> RequestDispatchFilterDelegate<in TRequest, TResponse>(IRequestContext<TRequest> requestContext, RequestDispatchFilterNextDelegate<TResponse> next);



public delegate Task<TResponse> RequestHandlingFilterNextDelegate<TResponse>();

public delegate Task<TResponse> RequestHandlingFilterDelegate<in TRequest, TResponse>(IRequestContext<TRequest> requestContext, RequestHandlingFilterNextDelegate<TResponse> next);


public delegate void RequestExceptionHandlerFallbackDelegate<in TResponse>(TResponse fallbackResponse);

public delegate Task<bool> RequestExceptionHandlerDelegate<in TRequest, out TResponse, in TException>(IRequestExceptionContext<TRequest, TException> requestContext, RequestExceptionHandlerFallbackDelegate<TResponse> fallbackCallback)
    where TException : Exception;



public delegate IAsyncEnumerable<TResponse> StreamDispatchFilterNextDelegate<out TResponse>(string? topic = null, Dictionary<string, object>? headers = null);

public delegate IAsyncEnumerable<TResponse> StreamDispatchFilterDelegate<in TRequest, TResponse>(IStreamContext<TRequest> streamContext, StreamDispatchFilterNextDelegate<TResponse> next);



public delegate IAsyncEnumerable<TResponse> StreamHandlingFilterNextDelegate<out TResponse>();

public delegate IAsyncEnumerable<TResponse> StreamHandlingFilterDelegate<in TRequest, TResponse>(IStreamContext<TRequest> streamContext, StreamHandlingFilterNextDelegate<TResponse> next);


public delegate void StreamExceptionHandlerFallbackDelegate<in TResponse>(TResponse fallbackResponse);

public delegate Task<bool> StreamExceptionHandlerDelegate<in TRequest, out TResponse, in TException>(IStreamExceptionContext<TRequest, TException> streamExceptionContext, StreamExceptionHandlerFallbackDelegate<TResponse> fallbackCallback)
    where TException : Exception;
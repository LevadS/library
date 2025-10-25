using LevadS.Classes;
using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestHandlersRegister
{
    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
    {
        if (!handler.CanHandleRequestWithTopic<TRequest, TResponse>(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(handler));
        }
        
        return AddRequestHandler<TRequest, TResponse, RequestHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern, 
            _ => new RequestHandlerDelegateWrapper<TRequest, TResponse>(handler));
    }

    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(Delegate handler)
        => AddRequestHandler<TRequest, TResponse>("*", handler);
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IRequestHandler<TRequest, TResponse>;

    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(
        Func<IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IRequestHandler<TRequest, TResponse>
        => AddRequestHandler<TRequest, TResponse, THandler>("*", handlerFactory);
}
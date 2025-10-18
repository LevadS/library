using LevadS.Classes;
using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestHandlersRegister
{
    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
        where TRequest : IRequest<TResponse>
    {
        if (!handler.CanHandleRequestWithTopic<TRequest, TResponse>(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(handler));
        }
        
        return AddRequestHandler<TRequest, TResponse, RequestHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern, 
            (serviceProvider, _) => new RequestHandlerDelegateWrapper<TRequest, TResponse>(serviceProvider, handler));
    }

    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse>(Delegate handler)
        where TRequest : IRequest<TResponse>
        => AddRequestHandler<TRequest, TResponse>("*", handler);
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IServiceProvider, IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>;

    IDisposableRequestHandlerBuilder<TRequest, TResponse> AddRequestHandler<TRequest, TResponse, THandler>(
        Func<IServiceProvider, IRequestContext<TRequest>, THandler>? handlerFactory = null)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
        => AddRequestHandler<TRequest, TResponse, THandler>("*", handlerFactory);
}
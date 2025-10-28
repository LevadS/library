using LevadS.Classes;
using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamHandlersRegister
{
    IDisposableStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse>(string topicPattern, Delegate handler)
    {
        if (!handler.CanHandleStreamWithTopic<TRequest, TResponse>(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(handler));
        }

        return AddStreamHandler<TRequest, TResponse, StreamHandlerDelegateWrapper<TRequest, TResponse>>(topicPattern,
            _ => new StreamHandlerDelegateWrapper<TRequest, TResponse>(handler));
    }

    IDisposableStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse>(Delegate handler)
        => AddStreamHandler<TRequest, TResponse>("*", handler);
    
    IDisposableStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern,
        Func<IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IStreamHandler<TRequest, TResponse>;

    IDisposableStreamHandlerBuilder<TRequest, TResponse> AddStreamHandler<TRequest, TResponse, THandler>(
        Func<IStreamContext<TRequest>, THandler>? handlerFactory = null)
        where THandler : class, IStreamHandler<TRequest, TResponse>
        => AddStreamHandler<TRequest, TResponse, THandler>("*", handlerFactory);
}
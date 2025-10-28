using LevadS.Services;
using LevadS.Interfaces;

namespace LevadS.Classes.Builders;

internal class HandlersRegister(IServiceRegister serviceRegister) : LevadSBuilder(serviceRegister), IHandlersRegister
{
    IDisposableMessageHandlerBuilder<TMessage> IMessageHandlersRegister.AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IMessageContext<TMessage>, THandler>? handlerFactory)
        => (IDisposableMessageHandlerBuilder<TMessage>)AddMessageHandler(topicPattern, handlerFactory);

    IDisposableRequestHandlerBuilder<TRequest, TResponse> IRequestHandlersRegister.AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IRequestContext<TRequest>, THandler>? handlerFactory)
        => (IDisposableRequestHandlerBuilder<TRequest, TResponse>)AddRequestHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);

    IDisposableStreamHandlerBuilder<TRequest, TResponse> IStreamHandlersRegister.AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IStreamContext<TRequest>, THandler>? handlerFactory)
        => (IDisposableStreamHandlerBuilder<TRequest, TResponse>)AddStreamHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);
}
using Microsoft.Extensions.DependencyInjection;
using LevadS.Services;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;

namespace LevadS.Classes.Builders;

internal class HandlersRegister(IDispatcher dispatcher) : LevadSBuilder(((Dispatcher)dispatcher).ServiceCollection.Clone()), IHandlersRegister
{
    private Dispatcher Dispatcher => (Dispatcher)dispatcher;
    
    protected override IMessageHandlerBuilder<TMessage> CreateMessageHandlerBuilder<TMessage>(string key, IEnumerable<ServiceDescriptor> serviceDescriptors)
        => new DisposableMessageHandlerBuilder<TMessage>(this, ServiceCollection, key, serviceDescriptors, Dispatcher);

    protected override IRequestHandlerBuilder<TRequest, TResponse> CreateRequestHandlerBuilder<TRequest, TResponse>(string key, IEnumerable<ServiceDescriptor> serviceDescriptors)
        => new DisposableRequestHandlerBuilder<TRequest, TResponse>(this, ServiceCollection, key, serviceDescriptors, Dispatcher);

    protected override IStreamHandlerBuilder<TRequest, TResponse> CreateStreamHandlerBuilder<TRequest, TResponse>(string key, IEnumerable<ServiceDescriptor> serviceDescriptors)
        => new DisposableStreamHandlerBuilder<TRequest, TResponse>(this, ServiceCollection, key, serviceDescriptors, Dispatcher);

    IDisposableMessageHandlerBuilder<TMessage> IMessageHandlersRegister.AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IServiceProvider, IMessageContext<TMessage>, THandler>? handlerFactory)
        => (IDisposableMessageHandlerBuilder<TMessage>)AddMessageHandler(topicPattern, handlerFactory);

    IDisposableRequestHandlerBuilder<TRequest, TResponse> IRequestHandlersRegister.AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IServiceProvider, IRequestContext<TRequest>, THandler>? handlerFactory)
        => (IDisposableRequestHandlerBuilder<TRequest, TResponse>)AddRequestHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);

    IDisposableStreamHandlerBuilder<TRequest, TResponse> IStreamHandlersRegister.AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IServiceProvider, IStreamContext<TRequest>, THandler>? handlerFactory)
        => (IDisposableStreamHandlerBuilder<TRequest, TResponse>)AddStreamHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);
}
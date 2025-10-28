using LevadS.Classes;
using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageHandlersRegister
{
    IDisposableMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage>(string topicPattern, Delegate handler)
        => AddMessageHandler<TMessage, MessageHandlerDelegateWrapper<TMessage>>(topicPattern, _ => new MessageHandlerDelegateWrapper<TMessage>(handler));
    
    IDisposableMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage>(Delegate handler)
        => AddMessageHandler<TMessage>("*", handler);
    
    IDisposableMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(string topicPattern,
        Func<IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>;

    IDisposableMessageHandlerBuilder<TMessage> AddMessageHandler<TMessage, THandler>(
        Func<IMessageContext<TMessage>, THandler>? handlerFactory = null)
        where THandler : class, IMessageHandler<TMessage>
        => AddMessageHandler<TMessage, THandler>("*", handlerFactory);
}
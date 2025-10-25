using LevadS.Classes;
using LevadS.Delegates;

namespace LevadS.Interfaces;

public interface IDisposableMessageHandlerBuilder<out TMessage>
{
    IDisposableMessageHandlerBuilder<TMessage> WithFilter(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => WithFilter<MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    IDisposableMessageHandlerBuilder<TMessage> WithFilter(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => WithFilter<MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    IDisposableMessageHandlerBuilder<TMessage> WithFilter<TFilter>(string topicPattern,
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    IDisposableMessageHandlerBuilder<TMessage> WithFilter<TFilter>(
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException>(string topicPattern, MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<MessageExceptionHandlerDelegateWrapper<TMessage, TException>>(topicPattern, p => new MessageExceptionHandlerDelegateWrapper<TMessage, TException>(exceptionHandlerDelegate));
    
    IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => WithFilter<MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>>(topicPattern, _ => new MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(exceptionHandlerFactory));

    IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable Build();
}
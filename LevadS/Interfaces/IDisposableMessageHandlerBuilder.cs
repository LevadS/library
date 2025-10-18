using LevadS.Classes;
using LevadS.Delegates;

namespace LevadS.Interfaces;

public interface IDisposableMessageHandlerBuilder<TMessage> : IMessageHandlerBuilder<TMessage>
{
    new IDisposableMessageHandlerBuilder<TMessage> WithFilter(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => WithFilter<MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    new IDisposableMessageHandlerBuilder<TMessage> WithFilter(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => WithFilter<MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    new IDisposableMessageHandlerBuilder<TMessage> WithFilter<TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    new IDisposableMessageHandlerBuilder<TMessage> WithFilter<TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    new IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException>(string topicPattern, MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<MessageExceptionHandlerDelegateWrapper<TMessage, TException>>(topicPattern, p => new MessageExceptionHandlerDelegateWrapper<TMessage, TException>(exceptionHandlerDelegate));
    
    new IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    new IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => WithFilter<MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>>(topicPattern, p => new MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(p, exceptionHandlerFactory));

    new IDisposableMessageHandlerBuilder<TMessage> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable Build();
}
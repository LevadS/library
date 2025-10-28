using LevadS.Classes;
using LevadS.Delegates;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

public interface IMessageHandlerBuilder<TMessage> : ILevadSBuilder
{
    IMessageHandlerBuilder<TMessage> WithFilter(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => WithFilter<MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    IMessageHandlerBuilder<TMessage> WithFilter(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => WithFilter<MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));
    
    IMessageHandlerBuilder<TMessage> WithFilter<TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    IMessageHandlerBuilder<TMessage> WithFilter<TFilter>(
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    IMessageHandlerBuilder<TMessage> WithExceptionHandler<TException>(string topicPattern, MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<MessageExceptionHandlerDelegateWrapper<TMessage, TException>>(topicPattern, p => new MessageExceptionHandlerDelegateWrapper<TMessage, TException>(exceptionHandlerDelegate));
    
    IMessageHandlerBuilder<TMessage> WithExceptionHandler<TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    IMessageHandlerBuilder<TMessage> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => WithFilter<MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>>(topicPattern, p => new MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(exceptionHandlerFactory));

    IMessageHandlerBuilder<TMessage> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
}
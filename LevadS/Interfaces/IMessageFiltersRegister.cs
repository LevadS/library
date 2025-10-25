using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageFiltersRegister
{
    IDisposable AddMessageFilter<TMessage>(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));

    IDisposable AddMessageFilter<TMessage>(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));

    IDisposable AddMessageFilter<TMessage, TFilter>(string topicPattern,
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    IDisposable AddMessageFilter<TMessage, TFilter>(
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => AddMessageFilter<TMessage, TFilter>("*", filterFactory);
}

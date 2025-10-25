using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageDispatchFiltersRegister
{
    IDisposable AddMessageDispatchFilter<TMessage>(string topicPattern, MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>(topicPattern, p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));

    IDisposable AddMessageDispatchFilter<TMessage>(MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>("*", p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));

    IDisposable AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern,
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>;

    IDisposable AddMessageDispatchFilter<TMessage, TFilter>(
        Func<IMessageContext<TMessage>, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>
        => AddMessageDispatchFilter<TMessage, TFilter>("*", filterFactory);
}

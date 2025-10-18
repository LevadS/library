using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageDispatchFiltersRegister
{
    IAsyncDisposable AddMessageDispatchFilter<TMessage>(string topicPattern, MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>(topicPattern, p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));

    IAsyncDisposable AddMessageDispatchFilter<TMessage>(MessageDispatchFilterDelegate<TMessage> filterDelegate)
        => AddMessageDispatchFilter<TMessage, MessageDispatchFilterWrapper<TMessage>>("*", p => new MessageDispatchFilterWrapper<TMessage>(filterDelegate));

    IAsyncDisposable AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>;

    IAsyncDisposable AddMessageDispatchFilter<TMessage, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageDispatchFilter<TMessage>
        => AddMessageDispatchFilter<TMessage, TFilter>("*", filterFactory);
}

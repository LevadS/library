using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageFiltersRegister
{
    IAsyncDisposable AddMessageFilter<TMessage>(string topicPattern, MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>(topicPattern, p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));

    IAsyncDisposable AddMessageFilter<TMessage>(MessageHandlingFilterDelegate<TMessage> filterDelegate)
        => AddMessageFilter<TMessage, MessageHandlingFilterWrapper<TMessage>>("*", p => new MessageHandlingFilterWrapper<TMessage>(filterDelegate));

    IAsyncDisposable AddMessageFilter<TMessage, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>;

    IAsyncDisposable AddMessageFilter<TMessage, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IMessageHandlingFilter<TMessage>
        => AddMessageFilter<TMessage, TFilter>("*", filterFactory);
}

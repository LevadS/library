using LevadS.Classes.Builders;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;

namespace LevadS.Classes;

internal class MessageHandlerBuilder<TMessage>(ILevadSBuilder levadSBuilder, IServiceRegister serviceRegister, string key, IDisposable? registration = null, Func<IDisposable>? registrationAction = null)
    : HandlerBuilderBase(levadSBuilder), IMessageHandlerBuilder<TMessage>, IDisposableMessageHandlerBuilder<TMessage>
{
    private readonly List<IDisposable> _disposables = registration != null ? [registration] : [];
    private readonly List<Action> _registrationActions = [];
    
    public virtual IMessageHandlerBuilder<TMessage> WithFilter<TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory)
        where TFilter : class, IMessageHandlingFilter<TMessage>
    {
        _disposables.Add(serviceRegister.AddMessageHandlingFilter<TMessage, TFilter>(topicPattern, filterFactory, key: key));

        return this;
    }

    public IAsyncDisposable Build()
    {
        if (registrationAction != null)
        {
            _registrationActions.Add(() => _disposables.Insert(0, registrationAction()));
        }

        foreach (var action in _registrationActions)
        {
            action();
        }
        
        return new DisposableContainer(_disposables.ToArray());
    }

    IDisposableMessageHandlerBuilder<TMessage> IDisposableMessageHandlerBuilder<TMessage>.WithFilter<TFilter>(
        string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory)
    {
        _registrationActions.Add(() => WithFilter(topicPattern, filterFactory));
        return this;
    }
}
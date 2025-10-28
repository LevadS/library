using LevadS.Classes.Builders;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

internal class StreamHandlerBuilder<TRequest, TResponse>(ILevadSBuilder levadSBuilder, IServiceRegister serviceRegister, string key, IDisposable? registration = null, Func<IDisposable>? registrationAction = null)
    : HandlerBuilderBase(levadSBuilder), IStreamHandlerBuilder<TRequest, TResponse>, IDisposableStreamHandlerBuilder<TRequest, TResponse>
{
    private readonly List<IDisposable> _disposables = registration != null ? [registration] : [];
    private readonly List<Action> _registrationActions = [];
    
    public virtual IStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
    {
        serviceRegister.AddStreamHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory, key);

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

    IDisposableStreamHandlerBuilder<TRequest, TResponse> IDisposableStreamHandlerBuilder<TRequest, TResponse>.WithFilter<TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
    {
        _registrationActions.Add(() => WithFilter(topicPattern, filterFactory));
        return this;
    }
}
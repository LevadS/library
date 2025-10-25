using LevadS.Classes.Builders;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;

namespace LevadS.Classes;

internal class RequestHandlerBuilder<TRequest, TResponse>(ILevadSBuilder levadSBuilder, IServiceRegister serviceRegister, string key, IDisposable? registration = null, Func<IDisposable>? registrationAction = null)
    : HandlerBuilderBase(levadSBuilder), IRequestHandlerBuilder<TRequest, TResponse>, IDisposableRequestHandlerBuilder<TRequest, TResponse>
{
    private readonly List<IDisposable> _disposables = registration != null ? [registration] : [];
    private readonly List<Action> _registrationActions = [];
    
    public virtual IRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
    {
        serviceRegister.AddRequestHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory, key);

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

    IDisposableRequestHandlerBuilder<TRequest, TResponse> IDisposableRequestHandlerBuilder<TRequest, TResponse>.WithFilter<TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
    {
        _registrationActions.Add(() => WithFilter(topicPattern, filterFactory));
        return this;
    }
}
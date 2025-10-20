using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamDispatchFiltersRegister
{
    IAsyncDisposable AddStreamDispatchFilter<TRequest, TResponse>(string topicPattern, StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddStreamDispatchFilter<TRequest, TResponse>(StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>("*", p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>;

    IAsyncDisposable AddStreamDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>
        => AddStreamDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IAsyncDisposable AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>;

    IAsyncDisposable AddStreamDispatchFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>
        => AddStreamDispatchFilter<TRequest, TFilter>("*", filterFactory);
}

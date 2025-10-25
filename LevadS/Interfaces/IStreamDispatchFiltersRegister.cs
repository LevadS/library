using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamDispatchFiltersRegister
{
    IDisposable AddStreamDispatchFilter<TRequest, TResponse>(string topicPattern, StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddStreamDispatchFilter<TRequest, TResponse>(StreamDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamDispatchFilter<TRequest, TResponse, StreamDispatchFilterWrapper<TRequest, TResponse>>("*", p => new StreamDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>;

    IDisposable AddStreamDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest, TResponse>
        => AddStreamDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IDisposable AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>;

    IDisposable AddStreamDispatchFilter<TRequest, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamDispatchFilter<TRequest>
        => AddStreamDispatchFilter<TRequest, TFilter>("*", filterFactory);
}

using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamFiltersRegister
{
    IDisposable AddStreamFilter<TRequest, TResponse>(string topicPattern, StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddStreamFilter<TRequest, TResponse>(StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>("*", p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>;

    IDisposable AddStreamFilter<TRequest, TResponse, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
        => AddStreamFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IDisposable AddStreamFilter<TRequest, TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>;

    IDisposable AddStreamFilter<TRequest, TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>
        => AddStreamFilter<TRequest, TFilter>("*", filterFactory);
}

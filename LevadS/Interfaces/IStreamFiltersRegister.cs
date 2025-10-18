using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamFiltersRegister
{
    IAsyncDisposable AddStreamFilter<TRequest, TResponse>(string topicPattern, StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddStreamFilter<TRequest, TResponse>(StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddStreamFilter<TRequest, TResponse, StreamHandlingFilterWrapper<TRequest, TResponse>>("*", p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>;

    IAsyncDisposable AddStreamFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
        => AddStreamFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IAsyncDisposable AddStreamFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>;

    IAsyncDisposable AddStreamFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest>
        => AddStreamFilter<TRequest, TFilter>("*", filterFactory);
}

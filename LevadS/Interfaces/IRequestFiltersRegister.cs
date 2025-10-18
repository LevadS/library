using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestFiltersRegister
{
    IAsyncDisposable AddRequestFilter<TRequest, TResponse>(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddRequestFilter<TRequest, TResponse>(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    IAsyncDisposable AddRequestFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => AddRequestFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IAsyncDisposable AddRequestFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>;

    IAsyncDisposable AddRequestFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>
        => AddRequestFilter<TRequest, TFilter>("*", filterFactory);
}

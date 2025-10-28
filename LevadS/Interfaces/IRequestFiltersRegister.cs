using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestFiltersRegister
{
    IDisposable AddRequestFilter<TRequest, TResponse>(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddRequestFilter<TRequest, TResponse>(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestFilter<TRequest, TResponse, RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    IDisposable AddRequestFilter<TRequest, TResponse, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => AddRequestFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IDisposable AddRequestFilter<TRequest, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>;

    IDisposable AddRequestFilter<TRequest, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest>
        => AddRequestFilter<TRequest, TFilter>("*", filterFactory);
}

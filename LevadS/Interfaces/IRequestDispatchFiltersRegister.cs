using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestDispatchFiltersRegister
{
    IAsyncDisposable AddRequestDispatchFilter<TRequest, TResponse>(string topicPattern, RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddRequestDispatchFilter<TRequest, TResponse>(RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        where TRequest : IRequest<TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>("*", p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IAsyncDisposable AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>;

    IAsyncDisposable AddRequestDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TRequest : IRequest<TResponse>
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IAsyncDisposable AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>;

    IAsyncDisposable AddRequestDispatchFilter<TRequest, TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>
        => AddRequestDispatchFilter<TRequest, TFilter>("*", filterFactory);
}

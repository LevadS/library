using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestDispatchFiltersRegister
{
    IDisposable AddRequestDispatchFilter<TRequest, TResponse>(string topicPattern, RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddRequestDispatchFilter<TRequest, TResponse>(RequestDispatchFilterDelegate<TRequest, TResponse> filterDelegate)
        => AddRequestDispatchFilter<TRequest, TResponse, RequestDispatchFilterWrapper<TRequest, TResponse>>("*", p => new RequestDispatchFilterWrapper<TRequest, TResponse>(filterDelegate));

    IDisposable AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>;

    IDisposable AddRequestDispatchFilter<TRequest, TResponse, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest, TResponse>
        => AddRequestDispatchFilter<TRequest, TResponse, TFilter>("*", filterFactory);

    IDisposable AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>;

    IDisposable AddRequestDispatchFilter<TRequest, TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestDispatchFilter<TRequest>
        => AddRequestDispatchFilter<TRequest, TFilter>("*", filterFactory);
}

using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class RequestDispatchFilterWrapper<TRequest, TResponse>(RequestDispatchFilterDelegate<TRequest, TResponse> filter) : IRequestDispatchFilter<TRequest, TResponse>
{
    private RequestDispatchFilterDelegate<TRequest, TResponse> Filter { get; } = filter;
    
    public Task<TResponse> InvokeAsync(IRequestContext<TRequest> context, RequestDispatchFilterNextDelegate<TResponse> next)
        => Filter.Invoke(context, next);
}
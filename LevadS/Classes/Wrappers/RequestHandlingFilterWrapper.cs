using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class RequestHandlingFilterWrapper<TRequest, TResponse>(RequestHandlingFilterDelegate<TRequest, TResponse> filter) : IRequestHandlingFilter<TRequest, TResponse>
{
    private RequestHandlingFilterDelegate<TRequest, TResponse> Filter { get; } = filter;
    
    public Task<TResponse> InvokeAsync(IRequestContext<TRequest> context, RequestHandlingFilterNextDelegate<TResponse> next)
        => Filter.Invoke(context, next);
}
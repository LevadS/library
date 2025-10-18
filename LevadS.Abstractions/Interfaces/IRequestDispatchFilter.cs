using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based request dispatch filters.
/// </summary>
/// <typeparam name="TRequest">Dispatched request message type</typeparam>
/// <typeparam name="TResponse">Requested response message type</typeparam>
public interface IRequestDispatchFilter<in TRequest, TResponse> : IFilter
{
    Task<TResponse> InvokeAsync(IRequestContext<TRequest> requestContext, RequestDispatchFilterNextDelegate<TResponse> next);
}

public interface IRequestDispatchFilter<in TRequest> : IRequestDispatchFilter<TRequest, object>;
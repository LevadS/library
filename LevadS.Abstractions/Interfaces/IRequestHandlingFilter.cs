using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based request handling filters.
/// </summary>
/// <typeparam name="TRequest">Handled request message type</typeparam>
/// <typeparam name="TResponse">Requested response message type</typeparam>
public interface IRequestHandlingFilter<in TRequest, TResponse> : IHandlingFilter
{
    Task<TResponse> InvokeAsync(IRequestContext<TRequest> requestContext, RequestHandlingFilterNextDelegate<TResponse> next);
}

public interface IRequestHandlingFilter<in TRequest> : IRequestHandlingFilter<TRequest, object>;
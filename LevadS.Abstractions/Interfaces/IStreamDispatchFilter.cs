using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based stream dispatch filters.
/// </summary>
/// <typeparam name="TRequest">Dispatched request message type</typeparam>
/// <typeparam name="TResponse">Requested streamed response message type</typeparam>
public interface IStreamDispatchFilter<in TRequest, TResponse> : IFilter
{
    IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamDispatchFilterNextDelegate<TResponse> next);
}

public interface IStreamDispatchFilter<in TRequest> : IStreamDispatchFilter<TRequest, object>;
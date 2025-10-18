using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based stream handling filters.
/// </summary>
/// <typeparam name="TRequest">Handled request message type</typeparam>
/// <typeparam name="TResponse">Requested streamed response message type</typeparam>
public interface IStreamHandlingFilter<in TRequest, TResponse> : IHandlingFilter
{
    IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamHandlingFilterNextDelegate<TResponse> next);
}

public interface IStreamHandlingFilter<in TRequest> : IStreamHandlingFilter<TRequest, object>;
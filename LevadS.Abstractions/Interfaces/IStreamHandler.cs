namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based stream handlers.
/// </summary>
/// <typeparam name="TRequest">Handled request message type</typeparam>
/// <typeparam name="TResponse">Requested streamed response message type</typeparam>
public interface IStreamHandler<in TRequest, out TResponse> : IHandler
    where TRequest : IRequest<TResponse>
{
    IAsyncEnumerable<TResponse> HandleAsync(IStreamContext<TRequest> streamContext, CancellationToken cancellationToken);
}
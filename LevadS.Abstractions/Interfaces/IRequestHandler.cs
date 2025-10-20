namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based request handlers.
/// </summary>
/// <typeparam name="TRequest">Handled request message type</typeparam>
/// <typeparam name="TResponse">Requested response message type</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IHandler
{
    Task<TResponse> HandleAsync(IRequestContext<TRequest> requestContext);
}
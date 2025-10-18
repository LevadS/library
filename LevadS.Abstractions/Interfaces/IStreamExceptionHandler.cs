using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based stream exception handlers.
/// </summary>
/// <typeparam name="TRequest">Request message type</typeparam>
/// <typeparam name="TResponse">Requested response message type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IStreamExceptionHandler<in TRequest, out TResponse, in TException> : IExceptionHandler
    where TException : Exception
{
    Task<bool> HandleAsync(IStreamExceptionContext<TRequest, TException> exceptionContext, StreamExceptionHandlerFallbackDelegate<TResponse> fallbackCallback);
}

public interface IStreamExceptionHandler<in TRequest, in TException> : IStreamExceptionHandler<TRequest, object, TException>
    where TException : Exception
{}
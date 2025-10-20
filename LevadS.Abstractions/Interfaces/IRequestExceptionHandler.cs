using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based request exception handlers.
/// </summary>
/// <typeparam name="TRequest">Request message type</typeparam>
/// <typeparam name="TResponse">Requested response message type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestExceptionHandler<in TRequest, out TResponse, in TException> : IExceptionHandler
    where TException : Exception
{
    Task<bool> HandleAsync(IRequestExceptionContext<TRequest, TException> exceptionContext, RequestExceptionHandlerFallbackDelegate<TResponse> fallbackCallback);
}

public interface IRequestExceptionHandler<in TRequest, in TException> : IRequestExceptionHandler<TRequest, object, TException>
    where TException : Exception
{}
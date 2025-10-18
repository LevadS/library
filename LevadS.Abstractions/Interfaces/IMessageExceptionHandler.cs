namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based message exception handlers.
/// </summary>
/// <typeparam name="TMessage">Message type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IMessageExceptionHandler<in TMessage, in TException> : IExceptionHandler
    where TException : Exception
{
    Task<bool> HandleAsync(IMessageExceptionContext<TMessage, TException> exceptionContext);
}
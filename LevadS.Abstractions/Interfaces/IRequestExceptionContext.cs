namespace LevadS.Interfaces;

/// <summary>
/// Interface used to expose request exception handling context.
/// </summary>
/// <typeparam name="TRequest">Request message type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IRequestExceptionContext<out TRequest, out TException> : IRequestContext<TRequest>
    where TException : Exception
{
    TException Exception { get; }
}
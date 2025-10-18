namespace LevadS.Interfaces;

/// <summary>
/// Interface used to expose stream exception handling context.
/// </summary>
/// <typeparam name="TRequest">Request message type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IStreamExceptionContext<out TRequest, out TException> : IStreamContext<TRequest>
    where TException : Exception
{
    TException Exception { get; }
}
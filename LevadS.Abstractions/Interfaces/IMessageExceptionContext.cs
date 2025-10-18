namespace LevadS.Interfaces;

/// <summary>
/// Interface used to expose message exception handling context.
/// </summary>
/// <typeparam name="TMessage">Message type</typeparam>
/// <typeparam name="TException">Exception type</typeparam>
public interface IMessageExceptionContext<out TMessage, out TException> : IMessageContext<TMessage>
    where TException : Exception
{
    TException Exception { get; }
}
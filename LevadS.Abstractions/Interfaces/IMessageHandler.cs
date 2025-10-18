namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based message handlers.
/// </summary>
/// <typeparam name="TMessage">Handled message type</typeparam>
public interface IMessageHandler<in TMessage> : IHandler
{
    Task HandleAsync(IMessageContext<TMessage> messageContext);
}
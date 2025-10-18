using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based message handling filters.
/// </summary>
/// <typeparam name="TMessage">Handled message type</typeparam>
public interface IMessageHandlingFilter<in TMessage> : IHandlingFilter
{
    Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageHandlingFilterNextDelegate next);
}
using LevadS.Delegates;

namespace LevadS.Interfaces;

/// <summary>
/// Interface to be implemented by type-based message dispatch filters.
/// </summary>
/// <typeparam name="TMessage">Dispatched message type</typeparam>
public interface IMessageDispatchFilter<in TMessage> : IFilter
{
    Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageDispatchFilterNextDelegate next);
}
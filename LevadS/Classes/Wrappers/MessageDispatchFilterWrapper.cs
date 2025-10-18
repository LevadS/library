using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class MessageDispatchFilterWrapper<TMessage>(MessageDispatchFilterDelegate<TMessage> filter) : IMessageDispatchFilter<TMessage>
{
    private MessageDispatchFilterDelegate<TMessage> Filter { get; } = filter;
    
    public Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageDispatchFilterNextDelegate next)
        => Filter.Invoke(messageContext, next);
}
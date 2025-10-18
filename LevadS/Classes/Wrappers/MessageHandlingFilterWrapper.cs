using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class MessageHandlingFilterWrapper<TMessage>(MessageHandlingFilterDelegate<TMessage> filter) : IMessageHandlingFilter<TMessage>
{
    private MessageHandlingFilterDelegate<TMessage> Filter { get; } = filter;
    
    public Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageHandlingFilterNextDelegate next)
        => Filter.Invoke(messageContext, next);
}
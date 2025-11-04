using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Filters;

[LevadSRegistration("orders:{operation}")]
public class OrderDispatchFilter([FromTopic] string operation) : IMessageDispatchFilter<Order>
{
    public Task InvokeAsync(IMessageContext<Order> messageContext, MessageDispatchFilterNextDelegate next)
    {
        next(topic: messageContext.Message.OrderPrice * messageContext.Message.OrderQuantity > 10000
            ? $"orders:premium:{operation}"
            : $"orders:standard:{operation}"
        );

        return Task.CompletedTask;
    }
}
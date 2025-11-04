using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Filters;

[LevadSRegistration("orders:#:create")]
public class OrderCreationFilter : IMessageHandlingFilter<Order>
{
    private static int _nextOrderId = 1;
    public Task InvokeAsync(IMessageContext<Order> messageContext, MessageHandlingFilterNextDelegate next)
    {
        next(headers: new Dictionary<string, object>(messageContext.Headers)
        {
            { "X-OrderId", _nextOrderId++ }
        });
        
        return Task.CompletedTask;
    }
}
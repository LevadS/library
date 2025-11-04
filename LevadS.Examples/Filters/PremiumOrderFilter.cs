using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Filters;

[LevadSRegistration("orders:premium:#")]
public class PremiumOrderFilter : IMessageHandlingFilter<Order>
{
    public Task InvokeAsync(IMessageContext<Order> messageContext, MessageHandlingFilterNextDelegate next)
    {
        next(headers: new Dictionary<string, object>(messageContext.Headers)
        {
            { "X-Packaging", "Premium packaging" },
            { "X-Shipment", "Free shipment" }
        });

        return Task.CompletedTask;
    }
}
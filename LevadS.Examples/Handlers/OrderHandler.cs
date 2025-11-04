using LevadS.Attributes;
using LevadS.Examples.Messages;
using LevadS.Examples.Services;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration("orders:#:{operation}")]
public class OrderHandler(
    [FromTopic] string operation,
    IDispatcher dispatcher,
    OrderRepository orderRepository
) : IMessageHandler<Order>
{
    public async Task HandleAsync(IMessageContext<Order> messageContext)
    {
        if (
            !messageContext.Headers.TryGetValue("X-OrderId", out var orderIdObject) ||
            orderIdObject is not int orderId
        )
        {
            throw new ApplicationException("Missing X-OrderId header");
        }
        
        switch (operation)
        {
            case "create":
                orderRepository.AddOrder(orderId, messageContext.Message);
                await dispatcher.PublishAsync($"Created order with id {orderId}", "notifications");
                break;
            
            case "update":
                orderRepository.UpdateOrder(messageContext.Message);
                await dispatcher.PublishAsync($"Updated order with id {orderId}", "notifications");
                break;
        } 
    }
}
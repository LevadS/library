using LevadS.Attributes;
using LevadS.Examples.Messages;
using LevadS.Examples.Services;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
public class OrderRequestHandler(OrderRepository orderRepository) : IRequestHandler<OrderRequest, Order>
{
    public Task<Order> HandleAsync(IRequestContext<OrderRequest> requestContext)
    {
        var order = orderRepository.GetOrderById(requestContext.Request.OrderId);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with id {requestContext.Request.OrderId} does not exist");
        }
        
        return Task.FromResult<Order>(order);
    }
}
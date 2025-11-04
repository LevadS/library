using LevadS.Examples.Messages;

namespace LevadS.Examples.Services;

public class OrderRepository
{
    private readonly List<Order> _orders = [];

    public void AddOrder(int orderId, Order order)
    {
        order = order with { Id = orderId };
        _orders.Add(order);
    }

    public void UpdateOrder(Order order)
    {
        var index = _orders.FindIndex(o => o.Id == order.Id);
        if (index == -1)
        {
            throw new KeyNotFoundException();
        }
        
        _orders[index] = order;
    }

    public Order? GetOrderById(int orderId)
    {
        return _orders.Find(o => o.Id == orderId);
    }
}
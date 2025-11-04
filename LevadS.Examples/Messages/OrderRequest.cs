namespace LevadS.Examples.Messages;

public record OrderRequest(int OrderId) : IRequest<Order>;
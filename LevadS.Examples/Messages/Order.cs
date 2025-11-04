namespace LevadS.Examples.Messages;

public record Order(int? Id, string CustomerName, DateTime OrderDate, decimal OrderPrice, decimal OrderQuantity);
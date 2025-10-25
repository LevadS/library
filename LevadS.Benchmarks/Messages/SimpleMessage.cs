namespace LevadS.Benchmarks.Messages;

public class SimpleMessage : MediatR.IRequest
{
    public int Number { get; set; }
}
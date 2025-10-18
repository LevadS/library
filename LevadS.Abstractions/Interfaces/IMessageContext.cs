namespace LevadS.Interfaces;

public interface IMessageContext : IContext
{}
    
/// <summary>
/// Interface used to expose message handling context.
/// </summary>
/// <typeparam name="TMessage">Handled message type</typeparam>
public interface IMessageContext<out TMessage> : IMessageContext
{
    TMessage Message { get; }
}
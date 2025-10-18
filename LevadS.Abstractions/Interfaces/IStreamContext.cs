namespace LevadS.Interfaces;

public interface IStreamContext : IContext
{ }

/// <summary>
/// Interface used to expose stream request handling context.
/// </summary>
/// <typeparam name="TRequest">Request message type</typeparam>
public interface IStreamContext<out TRequest> : IStreamContext
{
    TRequest Request { get; }
}
namespace LevadS.Interfaces;

public interface IRequestContext : IContext
{}

/// <summary>
/// Interface used to expose request handling context.
/// </summary>
/// <typeparam name="TRequest">Request message type</typeparam>
public interface IRequestContext<out TRequest> : IRequestContext
{
    TRequest Request { get; }
}
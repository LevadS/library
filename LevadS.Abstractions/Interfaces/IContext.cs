using LevadS.Enums;

namespace LevadS.Interfaces;

/// <summary>
/// Base interface for all kinds of message handling contexts.
/// </summary>
public interface IContext
{
    IReadOnlyDictionary<string, object> Headers { get; }
    
    string Topic { get; }
    
    IReadOnlyDictionary<string, object> CapturedTopicValues { get; }
    
    DispatchType DispatchType { get; }
    
    CancellationToken CancellationToken { get; }
    
    public IServiceProvider ServiceProvider { get; }
}
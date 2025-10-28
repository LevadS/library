using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class StreamDispatchEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IStreamDispatchFilter<TProvidedInput, TProvidedOutput> providedFilter) : IStreamDispatchFilter<TRequestedInput, TRequestedOutput>
{
    private StreamDispatchFilterNextDelegate<TRequestedOutput>? _nextDelegate;
    
    private async IAsyncEnumerable<TProvidedOutput> NextDelegate(string? topic = null, Dictionary<string, object>? headers = null)
    {
        await foreach (var result in _nextDelegate!(topic, headers))
        {
            yield return (TProvidedOutput)(object)result!;
        }
    }
    
    public async IAsyncEnumerable<TRequestedOutput> InvokeAsync(IStreamContext<TRequestedInput> streamContext,
        StreamDispatchFilterNextDelegate<TRequestedOutput> next)
    {
        _nextDelegate = next;
            
        var context = new StreamContext<TProvidedInput>((Context)streamContext);
        await foreach (var result in providedFilter.InvokeAsync(context, NextDelegate))
        {
            yield return (TRequestedOutput)(object)result!;
        }
    }
}

public class StreamDispatchEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new StreamDispatchEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IStreamDispatchFilter<TProvidedInput, TProvidedOutput>)service);
    
    public static StreamDispatchEnveloper Instance { get; } = new();
}
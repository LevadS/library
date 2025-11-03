using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class StreamHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IStreamHandlingFilter<TProvidedInput, TProvidedOutput> providedFilter) : IStreamHandlingFilter<TRequestedInput, TRequestedOutput>
{
    private StreamHandlingFilterNextDelegate<TRequestedOutput>? _nextDelegate;
    
    private async IAsyncEnumerable<TProvidedOutput> NextDelegate(Dictionary<string, object>? headers = null)
    {
        await foreach (var result in _nextDelegate!(headers))
        {
            yield return (TProvidedOutput)(object)result!;
        }
    }
    
    public async IAsyncEnumerable<TRequestedOutput> InvokeAsync(IStreamContext<TRequestedInput> streamContext,
        StreamHandlingFilterNextDelegate<TRequestedOutput> next)
    {
        _nextDelegate = next;
            
        var context = new StreamContext<TProvidedInput>((Context)streamContext);
        await foreach (var result in providedFilter.InvokeAsync(context, NextDelegate))
        {
            yield return (TRequestedOutput)(object)result!;
        }
    }
}

public class StreamHandlingEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new StreamHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IStreamHandlingFilter<TProvidedInput, TProvidedOutput>)service);
    
    public static StreamHandlingEnveloper Instance { get; } = new();
}
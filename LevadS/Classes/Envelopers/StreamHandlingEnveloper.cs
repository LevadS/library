using LevadS.Classes.Extensions;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class StreamHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IStreamHandlingFilter<TProvidedInput, TProvidedOutput> providedFilter) : IStreamHandlingFilter<TRequestedInput, TRequestedOutput>
    // where TRequestedInput : TProvidedInput
    // where TProvidedOutput : TRequestedOutput
{
    private StreamHandlingFilterNextDelegate<TRequestedOutput> nextDelegate;
    
    private async IAsyncEnumerable<TProvidedOutput> NextDelegate()
    {
        await foreach (var result in nextDelegate())
        {
            yield return (TProvidedOutput)(object)result;
        }
    }
    
    public async IAsyncEnumerable<TRequestedOutput> InvokeAsync(IStreamContext<TRequestedInput> streamContext,
        StreamHandlingFilterNextDelegate<TRequestedOutput> next)
    {
        nextDelegate = next;
            
        var context = new StreamContext<TProvidedInput>((Context)streamContext);
        await foreach (var result in providedFilter.InvokeAsync(context, NextDelegate))
        {
            yield return (TRequestedOutput)(object)result;
        }
    }
}

public class StreamHandlingEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new StreamHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IStreamHandlingFilter<TProvidedInput, TProvidedOutput>)service);
}
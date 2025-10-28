using LevadS.Delegates;
using LevadS.Interfaces;
#pragma warning disable CS8603 // Possible null reference return.

namespace LevadS.Classes.Envelopers;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

public class RequestDispatchEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IRequestDispatchFilter<TProvidedInput, TProvidedOutput> providedFilter) : IRequestDispatchFilter<TRequestedInput, TRequestedOutput>
{
    public async Task<TRequestedOutput> InvokeAsync(IRequestContext<TRequestedInput> messageContext, RequestDispatchFilterNextDelegate<TRequestedOutput> next)
        => (TRequestedOutput)(object)await providedFilter.InvokeAsync(
            new RequestContext<TProvidedInput>((Context)messageContext),
            async (t, h) => (TProvidedOutput)(object)await next(t, h)
        );
}
public class RequestDispatchEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new RequestDispatchEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IRequestDispatchFilter<TProvidedInput, TProvidedOutput>)service);
    
    public static RequestDispatchEnveloper Instance { get; } = new();
}
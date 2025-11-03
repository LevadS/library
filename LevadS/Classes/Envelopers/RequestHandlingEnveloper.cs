using LevadS.Delegates;
using LevadS.Interfaces;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.

namespace LevadS.Classes.Envelopers;

public class RequestHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IRequestHandlingFilter<TProvidedInput, TProvidedOutput> providedFilter) : IRequestHandlingFilter<TRequestedInput, TRequestedOutput>
{
    public async Task<TRequestedOutput> InvokeAsync(IRequestContext<TRequestedInput> requestContext, RequestHandlingFilterNextDelegate<TRequestedOutput> next)
        => (TRequestedOutput)(object)await providedFilter.InvokeAsync(
            new RequestContext<TProvidedInput>((Context)requestContext),
            async h => (TProvidedOutput)(object)await next(h)
        );
}

public class RequestHandlingEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new RequestHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IRequestHandlingFilter<TProvidedInput, TProvidedOutput>)service);
    
    public static RequestHandlingEnveloper Instance { get; } = new();
}
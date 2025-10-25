using LevadS.Classes.Extensions;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class RequestHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IRequestHandlingFilter<TProvidedInput, TProvidedOutput> providedFilter) : IRequestHandlingFilter<TRequestedInput, TRequestedOutput>
    // where TRequestedInput : TProvidedInput
    // where TProvidedOutput : TRequestedOutput
{
    public async Task<TRequestedOutput> InvokeAsync(IRequestContext<TRequestedInput> requestContext, RequestHandlingFilterNextDelegate<TRequestedOutput> next)
        => (TRequestedOutput)(object)await providedFilter.InvokeAsync(
            new RequestContext<TProvidedInput>((Context)requestContext),
            async () => (TProvidedOutput)(object)await next()
        );
    
    // public async Task<TRequestedOutput> InvokeAsync(IRequestContext<TRequestedInput> requestContext, RequestHandlingFilterNextDelegate<TRequestedOutput> next)
    // {
    //     var result = await providedFilter.InvokeAsync(
    //         new RequestContext<TProvidedInput>((Context)requestContext),
    //         async () =>
    //         {
    //             var result = await next();
    //             if (!result.TryConvert(typeof(TProvidedOutput), out var output))
    //             {
    //                 throw new InvalidOperationException("Wrong result type returned from request handling filter");
    //             }
    //
    //             return (TProvidedOutput)output;
    //         });
    //     
    //     if (!result.TryConvert(typeof(TRequestedOutput), out var output))
    //     {
    //         throw new InvalidOperationException("Wrong result type returned from request handling filter");
    //     }
    //
    //     return (TRequestedOutput)output;
    // }
}

public class RequestHandlingEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new RequestHandlingEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IRequestHandlingFilter<TProvidedInput, TProvidedOutput>)service);
}
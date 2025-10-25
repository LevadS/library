using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class RequestDispatchEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(IRequestDispatchFilter<TProvidedInput, TProvidedOutput> providedFilter) : IRequestDispatchFilter<TRequestedInput, TRequestedOutput>
    // where TRequestedInput : TProvidedInput
    // where TProvidedOutput : TRequestedOutput
{
    public async Task<TRequestedOutput> InvokeAsync(IRequestContext<TRequestedInput> messageContext, RequestDispatchFilterNextDelegate<TRequestedOutput> next)
        => (TRequestedOutput)(object)await providedFilter.InvokeAsync(
            new RequestContext<TProvidedInput>((Context)messageContext),
            async (t, h) => (TProvidedOutput)(object)await next(t, h)
        );


    // public async Task<TRequestedOutput> InvokeAsync(IRequestContext<TRequestedInput> requestContext,
    //     RequestDispatchFilterNextDelegate<TRequestedOutput> next)
    // {
    //     var result = await providedFilter.InvokeAsync(
    //         new RequestContext<TProvidedInput>((Context)requestContext),
    //         async (t, h) =>
    //         {
    //             var result = await next(t, h);
    //             if (!result.TryConvert(typeof(TProvidedOutput), out var output))
    //             {
    //                 throw new InvalidOperationException("Wrong result type returned from request dispatch filter");
    //             }
    //
    //             return (TProvidedOutput)output;
    //         });
    //     
    //     if (!result.TryConvert(typeof(TRequestedOutput), out var output))
    //     {
    //         throw new InvalidOperationException("Wrong result type returned from request dispatch filter");
    //     }
    //
    //     return (TRequestedOutput)output;
    // }
}
public class RequestDispatchEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new RequestDispatchEnvelope<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>((IRequestDispatchFilter<TProvidedInput, TProvidedOutput>)service);
}
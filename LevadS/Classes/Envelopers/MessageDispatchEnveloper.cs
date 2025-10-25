using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class MessageDispatchEnvelope<TProvidedInput, TRequestedInput>(IMessageDispatchFilter<TProvidedInput> providedFilter) : IMessageDispatchFilter<TRequestedInput>
    // where TRequestedInput : TProvidedInput
{
    public Task InvokeAsync(IMessageContext<TRequestedInput> messageContext, MessageDispatchFilterNextDelegate next)
        => providedFilter.InvokeAsync(new MessageContext<TProvidedInput>((Context)messageContext), next);
}

public class MessageDispatchEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new MessageDispatchEnvelope<TProvidedInput, TRequestedInput>((IMessageDispatchFilter<TProvidedInput>)service);
}
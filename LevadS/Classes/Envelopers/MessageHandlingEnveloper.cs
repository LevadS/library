using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class MessageHandlingEnvelope<TProvidedInput, TRequestedInput>(IMessageHandlingFilter<TProvidedInput> providedFilter) : IMessageHandlingFilter<TRequestedInput>
{
    public Task InvokeAsync(IMessageContext<TRequestedInput> messageContext, MessageHandlingFilterNextDelegate next)
        => providedFilter.InvokeAsync(new MessageContext<TProvidedInput>((Context)messageContext), next);
}

public class MessageHandlingEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => new MessageHandlingEnvelope<TProvidedInput, TRequestedInput>((IMessageHandlingFilter<TProvidedInput>)service);
    
    public static MessageHandlingEnveloper Instance { get; } = new();
}
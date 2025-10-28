using LevadS.Interfaces;

namespace LevadS.Classes.Envelopers;

public class NoopEnveloper : IServiceEnveloper
{
    object IServiceEnveloper.Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service)
        => service;
    
    public static NoopEnveloper Instance { get; } = new();
}
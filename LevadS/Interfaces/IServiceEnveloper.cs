namespace LevadS.Interfaces;

public interface IServiceEnveloper
{
    object Envelop<TProvidedInput, TRequestedInput, TProvidedOutput, TRequestedOutput>(object service);
}
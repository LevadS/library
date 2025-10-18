namespace LevadS.Tests.Registrations.Messages;

public class GenericMessage<T>
{
    public T GenericPayload { get; set; }
}
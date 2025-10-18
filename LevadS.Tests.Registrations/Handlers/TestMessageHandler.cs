using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Handlers;

[LevadSRegistration]
public class TestMessageHandler : IMessageHandler<TestMessage>
{
    public static bool Executed = false;

    public Task HandleAsync(IMessageContext<TestMessage> messageContext)
    {
        Executed = true;

        return Task.CompletedTask;
    }
}
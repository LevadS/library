using LevadS.Attributes;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration("exception")]
public class HandlerThatThrows : IMessageHandler<int>
{
    public Task HandleAsync(IMessageContext<int> messageContext)
    {
        throw new InvalidOperationException($"Error code: {messageContext.Message}");
    }
}
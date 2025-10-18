using System.Diagnostics;
using LevadS.Attributes;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
public class StringHandler : IMessageHandler<string>
{
    public Task HandleAsync(IMessageContext<string> messageContext)
    {
        Debug.WriteLine($"{typeof(StringHandler)}: {messageContext.Message}");
        
        return Task.CompletedTask;
    }
}
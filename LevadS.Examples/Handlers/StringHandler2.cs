using System.Diagnostics;
using LevadS.Attributes;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
public class StringHandler2 : IMessageHandler<string>
{
    public Task HandleAsync(IMessageContext<string> messageContext)
    {
        Debug.WriteLine(messageContext.Message);
        
        return Task.CompletedTask;
    }
}
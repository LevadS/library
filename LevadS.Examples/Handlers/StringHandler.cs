using System.Diagnostics;
using LevadS.Attributes;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration("topic:{value}")]
public class StringHandler(IHttpContextAccessor httpContent, IDispatcher dispatcher) : IMessageHandler<string>
{
    public async Task HandleAsync(IMessageContext<string> messageContext)
    {
        Debug.WriteLine($"{typeof(StringHandler)}: {messageContext.Message}, {httpContent.HttpContext?.Request.Path}");
        
        await dispatcher.RequestAsync(new StringStreamRequest(), "my-topic", messageContext.Headers.ToDictionary());
        
        // return Task.CompletedTask;
    }
}
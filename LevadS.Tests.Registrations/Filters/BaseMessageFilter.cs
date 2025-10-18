using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class BaseMessageFilter : IMessageHandlingFilter<BaseMessage>
{
    public static bool Executed = false;

    public Task InvokeAsync(IMessageContext<BaseMessage> messageContext, MessageHandlingFilterNextDelegate next)
    {
        Executed = true;
        
        return next();
    }
}
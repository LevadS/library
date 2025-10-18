using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class ObjectMessageFilter : IMessageHandlingFilter<object>
{
    public static bool Executed = false;

    public Task InvokeAsync(IMessageContext<object> messageContext, MessageHandlingFilterNextDelegate next)
    {
        Executed = true;
        
        return next();
    }
}
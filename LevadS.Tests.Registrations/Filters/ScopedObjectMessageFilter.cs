using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Delegates;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
[LevadSFilterFor<LevadS.Tests.Handlers.TestMessageHandler>]
public class ScopedObjectMessageFilter : IMessageHandlingFilter<object>
{
    public static bool Executed;

    public Task InvokeAsync(IMessageContext<object> messageContext, MessageHandlingFilterNextDelegate next)
    {
        Executed = true;
        return next();
    }
}

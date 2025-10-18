using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class ObjectMessageDispatchFilter : IMessageDispatchFilter<object>
{
    public static bool Executed = false;

    public Task InvokeAsync(IMessageContext<object> messageContext, MessageDispatchFilterNextDelegate next)
    {
        Executed = true;
        
        return next();
    }
}
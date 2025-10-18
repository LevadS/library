using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class ObjectRequestFilter : IRequestHandlingFilter<object, object>
{
    public static bool Executed = false;
    
    public Task<object> InvokeAsync(IRequestContext<object> requestContext, RequestHandlingFilterNextDelegate<object> next)
    {
        Executed = true;
        
        return next();
    }
}
using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
[LevadSFilterFor<LevadS.Tests.Handlers.TestRequestHandler>]
public class ScopedObjectRequestFilter : IRequestHandlingFilter<object>
{
    public static bool Executed;

    public Task<object> InvokeAsync(IRequestContext<object> requestContext, RequestHandlingFilterNextDelegate<object> next)
    {
        Executed = true;
        return next();
    }
}

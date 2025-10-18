using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration("short")]
public class ObjectShortCircuitFilter : IRequestHandlingFilter<object>
{
    public Task<object> InvokeAsync(IRequestContext<object> requestContext, RequestHandlingFilterNextDelegate<object> next)
        => Task.FromResult<object>(123);
}

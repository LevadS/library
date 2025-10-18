using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class ObjectRequestDispatchFilter : IRequestDispatchFilter<object, object>
{
    public static bool Executed;

    public Task<object> InvokeAsync(IRequestContext<object> requestContext, RequestDispatchFilterNextDelegate<object> next)
    {
        Executed = true;
        return next();
    }
}

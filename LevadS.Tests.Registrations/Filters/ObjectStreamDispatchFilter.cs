using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class ObjectStreamDispatchFilter : IStreamDispatchFilter<object, object>
{
    public static bool Executed;

    public IAsyncEnumerable<object> InvokeAsync(IStreamContext<object> streamContext, StreamDispatchFilterNextDelegate<object> next)
    {
        Executed = true;
        return next();
    }
}

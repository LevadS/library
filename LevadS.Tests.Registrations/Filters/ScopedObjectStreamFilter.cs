using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
[LevadSFilterFor<LevadS.Tests.Handlers.TestRequestHandler>]
public class ScopedObjectStreamFilter : IStreamHandlingFilter<object>
{
    public static bool Executed;

    public IAsyncEnumerable<object> InvokeAsync(IStreamContext<object> streamContext, StreamHandlingFilterNextDelegate<object> next)
    {
        Executed = true;
        return next();
    }
}

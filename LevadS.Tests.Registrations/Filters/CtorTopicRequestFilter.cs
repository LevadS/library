using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration("di:{n:int}")]
public class CtorTopicRequestFilter : IRequestHandlingFilter<object>
{
    public class TestReq : IRequest<int> { }

    public static int Seen;

    private readonly int _n;

    public CtorTopicRequestFilter([FromTopic("n")] int n)
    {
        _n = n;
    }

    public Task<object> InvokeAsync(IRequestContext<object> requestContext, RequestHandlingFilterNextDelegate<object> next)
    {
        Seen = _n;
        return next();
    }
}

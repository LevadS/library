using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Registrations.Filters;

[LevadSRegistration]
public class BaseRequestFilter : IRequestHandlingFilter<BaseRequest, TestResponse>
{
    public static bool Executed = false;

    public Task<TestResponse> InvokeAsync(IRequestContext<BaseRequest> requestContext, RequestHandlingFilterNextDelegate<TestResponse> next)
    {
        Executed = true;
        
        return next();
    }
}
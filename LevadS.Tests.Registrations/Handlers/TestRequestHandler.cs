using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Handlers;

[LevadSRegistration]
public class TestRequestHandler : IRequestHandler<TestRequest, TestResponse>
{
    public Task<TestResponse> HandleAsync(IRequestContext<TestRequest> requestContext)
    {
        return Task.FromResult(new TestResponse());
    }
}
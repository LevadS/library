using LevadS.Attributes;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
public class VariantHandler : IRequestHandler<BaseRequest, BaseResponse>
{
    public Task<BaseResponse> HandleAsync(IRequestContext<BaseRequest> requestContext)
    {
        return Task.FromResult<BaseResponse>(new InheritedResponse());
    }
}
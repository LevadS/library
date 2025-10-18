using LevadS.Interfaces;
using LevadS.Classes.Extensions;

namespace LevadS.Classes;

internal class RequestHandlerDelegateWrapper<TRequest, TResponse>(IServiceProvider serviceProvider, Delegate handler) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private Delegate Handler { get; } = handler;

    public Task<TResponse> HandleAsync(IRequestContext<TRequest> requestContext)
        => Handler.HandleRequestWithTopicAsync<TRequest, TResponse>(serviceProvider, requestContext);
}
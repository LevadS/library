using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicRequestHandlingFilter<TRequest, TResponse>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern, string key)
    : TopicMessageHandlingFilter<TRequest>(serviceProvider, serviceFactory, topicPattern, key), ITopicRequestHandlingFilter<TRequest, TResponse>
    // where TRequest : IRequest<TResponse>
{
    public Task<TResponse> InvokeAsync(IRequestContext<TRequest> requestContext, RequestHandlingFilterNextDelegate<TResponse> next)
        => ((IRequestHandlingFilter<TRequest, TResponse>)ServiceObject).InvokeAsync(requestContext, next);
}
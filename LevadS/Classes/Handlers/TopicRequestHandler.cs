using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicRequestHandler<TRequest, TResponse>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern, string key)
    : TopicMessageHandler<TRequest>(serviceProvider, serviceFactory, topicPattern, key), ITopicRequestHandler<TRequest, TResponse>
{
    public Task<TResponse> HandleAsync(IRequestContext<TRequest> requestContext)
        => ((IRequestHandler<TRequest, TResponse>)ServiceObject).HandleAsync(requestContext);
}
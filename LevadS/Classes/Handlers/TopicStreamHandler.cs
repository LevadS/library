using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicStreamHandler<TRequest, TResponse>(IServiceProvider serviceProvider,Func<TopicHandler, object> serviceFactory, string topicPattern, string key)
    : TopicRequestHandler<TRequest, TResponse>(serviceProvider, serviceFactory, topicPattern, key), ITopicStreamHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public IAsyncEnumerable<TResponse> HandleAsync(IStreamContext<TRequest> streamContext, CancellationToken cancellationToken)
        => ((IStreamHandler<TRequest, TResponse>)ServiceObject).HandleAsync(streamContext, cancellationToken);
}
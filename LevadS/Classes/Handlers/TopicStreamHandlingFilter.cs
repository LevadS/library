using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicStreamHandlingFilter<TRequest, TResponse>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern, string key)
    : TopicRequestHandlingFilter<TRequest, TResponse>(serviceProvider, serviceFactory, topicPattern, key), ITopicStreamHandlingFilter<TRequest, TResponse>
    // where TRequest : IRequest<TResponse>
{
    public IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamHandlingFilterNextDelegate<TResponse> next)
        => ((IStreamHandlingFilter<TRequest, TResponse>)ServiceObject).InvokeAsync(streamContext, next);
}
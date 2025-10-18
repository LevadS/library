using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicStreamDispatchFilter<TRequest, TResponse>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern)
    : TopicRequestDispatchFilter<TRequest, TResponse>(serviceProvider, serviceFactory, topicPattern), ITopicStreamDispatchFilter<TRequest, TResponse>
{
    public IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamDispatchFilterNextDelegate<TResponse> next)
        => ((IStreamDispatchFilter<TRequest, TResponse>)ServiceObject).InvokeAsync(streamContext, next);
}
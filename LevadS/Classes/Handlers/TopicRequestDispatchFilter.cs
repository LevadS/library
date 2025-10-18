using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicRequestDispatchFilter<TRequest, TResponse>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern)
    : TopicMessageDispatchFilter<TRequest>(serviceProvider, serviceFactory, topicPattern), ITopicRequestDispatchFilter<TRequest, TResponse>
    // where TRequest : IRequest<TResponse>
{
    public Task<TResponse> InvokeAsync(IRequestContext<TRequest> requestContext, RequestDispatchFilterNextDelegate<TResponse> next)
        => ((IRequestDispatchFilter<TRequest, TResponse>)ServiceObject).InvokeAsync(requestContext, next);
}
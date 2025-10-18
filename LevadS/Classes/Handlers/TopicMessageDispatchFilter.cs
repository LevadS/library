using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicMessageDispatchFilter<TMessage>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern)
    : TopicHandler(serviceProvider, serviceFactory, topicPattern, string.Empty), ITopicMessageDispatchFilter<TMessage>
{
    public Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageDispatchFilterNextDelegate next)
        => ((IMessageDispatchFilter<TMessage>)ServiceObject).InvokeAsync(messageContext, next);
}
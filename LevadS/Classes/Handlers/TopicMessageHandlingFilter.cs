using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicMessageHandlingFilter<TMessage>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern, string key)
    : TopicHandler(serviceProvider, serviceFactory, topicPattern, key), ITopicMessageHandlingFilter<TMessage>
{
    public Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageHandlingFilterNextDelegate next)
        => ((IMessageHandlingFilter<TMessage>)ServiceObject).InvokeAsync(messageContext, next);
}
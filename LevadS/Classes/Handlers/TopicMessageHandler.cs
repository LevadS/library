using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicMessageHandler<TMessage>(IServiceProvider serviceProvider, Func<TopicHandler, object> serviceFactory, string topicPattern, string key)
    : TopicHandler(serviceProvider, serviceFactory, topicPattern, key), ITopicMessageHandler<TMessage>
{
    public Task HandleAsync(IMessageContext<TMessage> messageContext)
        => ((IMessageHandler<TMessage>)ServiceObject).HandleAsync(messageContext);
}
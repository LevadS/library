using LevadS.Interfaces;
using LevadS.Classes.Extensions;

namespace LevadS.Classes;

internal class MessageHandlerDelegateWrapper<TMessage>(IServiceProvider serviceProvider, Delegate handler) : IMessageHandler<TMessage>
{
    private Delegate Handler { get; } = handler;

    public Task HandleAsync(IMessageContext<TMessage> messageContext)
        => Handler.HandleMessageWithTopicAsync(serviceProvider, messageContext);
}
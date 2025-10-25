using LevadS.Benchmarks.Messages;
using LevadS.Interfaces;

namespace LevadS.Benchmarks.Handlers;

public class SimpleMessageHandler : IMessageHandler<SimpleMessage>, MediatR.IRequestHandler<SimpleMessage>
{
    public Task HandleAsync(IMessageContext<SimpleMessage> messageContext)
    {
        return Task.CompletedTask;
    }

    public Task Handle(SimpleMessage request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
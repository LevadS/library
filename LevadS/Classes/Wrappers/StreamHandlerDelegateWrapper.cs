using LevadS.Interfaces;
using LevadS.Classes.Extensions;

namespace LevadS.Classes;

internal class StreamHandlerDelegateWrapper<TRequest, TResponse>(Delegate handler) : IStreamHandler<TRequest, TResponse>
{
    private Delegate Handler { get; } = handler;

    public IAsyncEnumerable<TResponse> HandleAsync(IStreamContext<TRequest> streamContext, CancellationToken cancellationToken)
        => Handler.HandleStreamWithTopicAsync<TRequest, TResponse>(streamContext.ServiceProvider, streamContext);
}
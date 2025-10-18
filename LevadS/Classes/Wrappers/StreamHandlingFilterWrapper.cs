using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class StreamHandlingFilterWrapper<TRequest, TResponse>(StreamHandlingFilterDelegate<TRequest, TResponse> filter) : IStreamHandlingFilter<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private StreamHandlingFilterDelegate<TRequest, TResponse> Filter { get; } = filter;
    
    public IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamHandlingFilterNextDelegate<TResponse> next)
        => Filter.Invoke(streamContext, next);
}
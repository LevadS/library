using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class StreamDispatchFilterWrapper<TRequest, TResponse>(StreamDispatchFilterDelegate<TRequest, TResponse> filter) : IStreamDispatchFilter<TRequest, TResponse>
{
    private StreamDispatchFilterDelegate<TRequest, TResponse> Filter { get; } = filter;
    
    public IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamDispatchFilterNextDelegate<TResponse> next)
        => Filter.Invoke(streamContext, next);
}
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class TopicStreamHandlingFilterVariantWrapper<TRequest, TResponse>(object filter)
    : BaseTopicFilterVariantWrapper(filter), ITopicStreamHandlingFilter<TRequest, TResponse>
{
    private readonly object _filter = filter;
    private StreamHandlingFilterNextDelegate<TResponse>? _next;

    public async IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> context,
        StreamHandlingFilterNextDelegate<TResponse> next)
    {
        _next = next;
        if (context is not Context baseContext)
            throw new InvalidOperationException("Context must derive from Context");
        var streamContext = new StreamContext<object>(baseContext);
        var method = _filter.GetType().GetMethods().First(m => m.Name == "InvokeAsync");
        var invoked = method.Invoke(_filter, new object[] { streamContext, (StreamHandlingFilterNextDelegate<object>)Bridge });
        if (invoked is not IAsyncEnumerable<object> resultEnum)
        {
            throw new InvalidOperationException("InvokeAsync must return IAsyncEnumerable<object>");
        }
        await foreach (var response in resultEnum)
        {
            yield return (TResponse)response!;
        }

        yield break;

        async IAsyncEnumerable<object> Bridge()
        {
            await foreach (var r in _next!()) yield return r!;
        }
    }
}
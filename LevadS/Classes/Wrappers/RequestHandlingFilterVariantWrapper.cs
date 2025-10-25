using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

#pragma warning disable CS9107
#pragma warning disable CS8603

internal class RequestHandlingFilterVariantWrapper<TRequest, TResponse>(object filter)
    : BaseTopicFilterVariantWrapper(filter), IRequestHandlingFilter<TRequest, TResponse>
{
    public async Task<TResponse> InvokeAsync(IRequestContext<TRequest> context,
        RequestHandlingFilterNextDelegate<TResponse> next)
    {
        if (context is not Context baseContext)
            throw new InvalidOperationException("Context must derive from Context");
        var requestContext = new RequestContext<object>(baseContext);
        var method = filter.GetType().GetMethods().First(m => m.Name == "InvokeAsync");
        var invoked = method.Invoke(filter, [requestContext, (RequestHandlingFilterNextDelegate<object>)(async () => await next())]);
        if (invoked is not Task resultTask)
        {
            throw new InvalidOperationException("InvokeAsync must return a Task");
        }
        var result = await AwaitAndCastAsync<object>(resultTask);

        if (result is TResponse cast) return cast;
        if (result is null) return default!;
        return (TResponse)result;
    }
}

#pragma warning restore CS8603
#pragma warning restore CS9107
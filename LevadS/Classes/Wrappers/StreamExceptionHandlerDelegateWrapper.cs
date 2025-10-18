using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandler) : IStreamHandlingFilter<TRequest, TResponse>, IExceptionHandler
    where TException : Exception
{
    public async IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> streamContext, StreamHandlingFilterNextDelegate<TResponse> next)
    {
        var enumerator = next().GetAsyncEnumerator(streamContext.CancellationToken);
        var more = true;
        while (more && !streamContext.CancellationToken.IsCancellationRequested)
        {
            List<TResponse> responses = [];

            try
            {
                more = await enumerator.MoveNextAsync();
                if (more)
                {
                    responses.Add(enumerator.Current);
                }
            }
            catch (TException e)
            {
                await exceptionHandler.Invoke(new StreamExceptionContext<TRequest, TException>((StreamContext<TRequest>)streamContext, e), Callback);
            }
            
            foreach (var response in responses)
            {
                yield return response;
            }

            continue;

            void Callback(TResponse r) => responses.Add(r);
        }
    }
}

internal class StreamExceptionHandlerDelegateWrapper<TRequest, TException>(
    StreamExceptionHandlerDelegate<TRequest, object, TException> exceptionHandler)
    : StreamExceptionHandlerDelegateWrapper<TRequest, object, TException>(exceptionHandler), IStreamHandlingFilter<TRequest>
    where TException : Exception;
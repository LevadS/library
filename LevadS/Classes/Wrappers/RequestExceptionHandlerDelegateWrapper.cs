using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandler) : IRequestHandlingFilter<TRequest, TResponse>, IExceptionHandler
    where TException : Exception
{
    public async Task<TResponse> InvokeAsync(IRequestContext<TRequest> requestContext, RequestHandlingFilterNextDelegate<TResponse> next)
    {
        TResponse? response = default;
        try
        {
             response = await next();
        }
        catch (TException e)
        {
            var fallbackProvided = false;
            void Callback(TResponse r)
            {
                fallbackProvided = true;
                response = r;
            }

            var handled = await exceptionHandler.Invoke(new RequestExceptionContext<TRequest, TException>((RequestContext<TRequest>)requestContext, e), Callback);

            switch (handled)
            {
                case true when !fallbackProvided || (response?.Equals(default(TResponse)) ?? false):
                    throw new InvalidOperationException("Fallback response not provided");
                case false:
                    throw;
            }
        }

        return response!;
    }
}

using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(
    Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
    : RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>((ctx, callback) =>
    {
        var handler = exceptionHandlerFactory?.Invoke(ctx) ?? ctx.ServiceProvider.CreateInstanceWithTopic<TExceptionHandler>((Context)ctx);
        return handler.HandleAsync(ctx, callback);
    })
    where TException : Exception
    where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>;

internal class RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
    : RequestExceptionHandlerWrapper<TRequest, object, TException, TExceptionHandler>(exceptionHandlerFactory), IRequestHandlingFilter<TRequest>
    where TException : Exception
    where TExceptionHandler : class, IRequestExceptionHandler<TRequest, object, TException>;

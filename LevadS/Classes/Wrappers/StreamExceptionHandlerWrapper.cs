using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(
    Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
    : StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>((ctx, callback) =>
    {
        var handler = exceptionHandlerFactory?.Invoke(ctx) ?? ctx.ServiceProvider.CreateInstanceWithTopic<TExceptionHandler>((Context)ctx);
        return handler.HandleAsync(ctx, callback);
    })
    where TException : Exception
    where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>;

internal class StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
    : StreamExceptionHandlerWrapper<TRequest, object, TException, TExceptionHandler>(exceptionHandlerFactory), IStreamHandlingFilter<TRequest>
    where TException : Exception
    where TExceptionHandler : class, IStreamExceptionHandler<TRequest, object, TException>;
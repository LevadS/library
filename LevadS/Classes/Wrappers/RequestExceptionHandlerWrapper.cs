using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

internal class RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(
    IServiceProvider serviceProvider,
    Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
    : RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>((ctx, callback) =>
    {
        var handler = exceptionHandlerFactory?.Invoke(serviceProvider) ??
                      ActivatorUtilities.CreateInstance<TExceptionHandler>(serviceProvider);
        return handler.HandleAsync(ctx, callback);
    })
    where TException : Exception
    where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>;

internal class RequestExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(IServiceProvider serviceProvider, Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
    : RequestExceptionHandlerWrapper<TRequest, object, TException, TExceptionHandler>(serviceProvider, exceptionHandlerFactory), IRequestHandlingFilter<TRequest>
    where TException : Exception
    where TExceptionHandler : class, IRequestExceptionHandler<TRequest, object, TException>;

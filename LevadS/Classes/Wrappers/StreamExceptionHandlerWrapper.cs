using System.Diagnostics;
using LevadS.Delegates;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

internal class StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(
    IServiceProvider serviceProvider,
    Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
    : StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>((ctx, callback) =>
    {
        var handler = exceptionHandlerFactory?.Invoke(serviceProvider) ??
                      ActivatorUtilities.CreateInstance<TExceptionHandler>(serviceProvider);
        return handler.HandleAsync(ctx, callback);
    })
    where TException : Exception
    where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>;

internal class StreamExceptionHandlerWrapper<TRequest, TException, TExceptionHandler>(IServiceProvider serviceProvider, Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
    : StreamExceptionHandlerWrapper<TRequest, object, TException, TExceptionHandler>(serviceProvider, exceptionHandlerFactory), IStreamHandlingFilter<TRequest>
    where TException : Exception
    where TExceptionHandler : class, IStreamExceptionHandler<TRequest, object, TException>;
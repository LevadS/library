using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

internal class MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(IServiceProvider serviceProvider, Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
    : MessageExceptionHandlerDelegateWrapper<TMessage, TException>(ctx =>
    {
        var handler = exceptionHandlerFactory?.Invoke(serviceProvider) ?? ActivatorUtilities.CreateInstance<TExceptionHandler>(serviceProvider);
        return handler.HandleAsync(ctx);
    })
    where TException : Exception
    where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>;
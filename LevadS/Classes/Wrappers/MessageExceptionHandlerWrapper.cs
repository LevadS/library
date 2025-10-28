using LevadS.Classes.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class MessageExceptionHandlerWrapper<TMessage, TException, TExceptionHandler>(
    Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null
)
    : MessageExceptionHandlerDelegateWrapper<TMessage, TException>(ctx =>
    {
        var handler = exceptionHandlerFactory?.Invoke(ctx) ?? ctx.ServiceProvider.CreateInstanceWithTopic<TExceptionHandler>((Context)ctx);
        return handler.HandleAsync(ctx);
    })
    where TException : Exception
    where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>;
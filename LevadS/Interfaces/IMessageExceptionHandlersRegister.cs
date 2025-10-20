using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageExceptionHandlersRegister
{
    IAsyncDisposable AddMessageExceptionHandler<TMessage, TException>(string topicPattern, MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception;
    
    IAsyncDisposable AddMessageExceptionHandler<TMessage, TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddMessageExceptionHandler("*", exceptionHandlerDelegate);

    IAsyncDisposable AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>;

    IAsyncDisposable AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>("*", exceptionHandlerFactory);
}

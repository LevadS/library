using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IMessageExceptionHandlersRegister
{
    IDisposable AddMessageExceptionHandler<TMessage, TException>(string topicPattern, MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception;
    
    IDisposable AddMessageExceptionHandler<TMessage, TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddMessageExceptionHandler("*", exceptionHandlerDelegate);

    IDisposable AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>;

    IDisposable AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(
        Func<IMessageExceptionContext<TMessage, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IMessageExceptionHandler<TMessage, TException>
        => AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>("*", exceptionHandlerFactory);
}

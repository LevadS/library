using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamExceptionHandlersRegister
{
    IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern, StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception;
    
    IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddStreamExceptionHandler("*", exceptionHandlerDelegate);
    
    IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>;

    IDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IDisposable AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>;

    IDisposable AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
}

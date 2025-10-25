using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestExceptionHandlersRegister
{
    IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception;
    
    IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddRequestExceptionHandler("*", exceptionHandlerDelegate);
    
    IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>;

    IDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IDisposable AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>;

    IDisposable AddRequestExceptionHandler<TRequest, TExceptionHandler, TException>(
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
}

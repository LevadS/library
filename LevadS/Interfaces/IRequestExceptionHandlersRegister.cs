using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IRequestExceptionHandlersRegister
{
    IAsyncDisposable AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception;
    
    IAsyncDisposable AddRequestExceptionHandler<TRequest, TResponse, TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddRequestExceptionHandler("*", exceptionHandlerDelegate);
    
    IAsyncDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>;

    IAsyncDisposable AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>;

    IAsyncDisposable AddRequestExceptionHandler<TRequest, TExceptionHandler, TException>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TException>
        => AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
}

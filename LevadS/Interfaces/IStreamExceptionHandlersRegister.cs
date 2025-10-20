using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS;

public interface IStreamExceptionHandlersRegister
{
    IAsyncDisposable AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern, StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception;
    
    IAsyncDisposable AddStreamExceptionHandler<TRequest, TResponse, TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => AddStreamExceptionHandler("*", exceptionHandlerDelegate);
    
    IAsyncDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>;

    IAsyncDisposable AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>;

    IAsyncDisposable AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TException>
        => AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>("*", exceptionHandlerFactory);
}

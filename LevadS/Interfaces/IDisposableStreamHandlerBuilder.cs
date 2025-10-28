using LevadS.Classes;
using LevadS.Delegates;

namespace LevadS.Interfaces;

public interface IDisposableStreamHandlerBuilder<out TRequest, TResponse> : IStreamHandlerBuilder<TRequest, TResponse>
{
    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithFilter(string topicPattern, StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<StreamHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, _ => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithFilter(StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<StreamHandlingFilterWrapper<TRequest, TResponse>>("*", _ => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern,
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>;

    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(
        Func<IStreamContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(string topicPattern, StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, _ => new StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => WithFilter<StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(topicPattern, _ => new StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(exceptionHandlerFactory));

    new IDisposableStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IStreamExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable Build();
}
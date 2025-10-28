using LevadS.Classes;
using LevadS.Delegates;

namespace LevadS.Interfaces;

public interface IDisposableRequestHandlerBuilder<out TRequest, TResponse>
{
    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern,
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(
        Func<IRequestContext<TRequest>, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => WithFilter<RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(topicPattern, p => new RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(exceptionHandlerFactory));

    IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IRequestExceptionContext<TRequest, TException>, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable Build();
}
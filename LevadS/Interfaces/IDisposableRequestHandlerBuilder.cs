using LevadS.Classes;
using LevadS.Delegates;

namespace LevadS.Interfaces;

public interface IDisposableRequestHandlerBuilder<TRequest, TResponse> : IRequestHandlerBuilder<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => WithFilter<RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(topicPattern, p => new RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(p, exceptionHandlerFactory));

    new IDisposableRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
    
    IAsyncDisposable Build();
}
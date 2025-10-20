using LevadS.Classes;
using LevadS.Delegates;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

public interface IRequestHandlerBuilder<out TRequest, TResponse> : ILevadSBuilder
{
    IRequestHandlerBuilder<TRequest, TResponse> WithFilter(string topicPattern, RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<RequestHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    IRequestHandlerBuilder<TRequest, TResponse> WithFilter(RequestHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<RequestHandlingFilterWrapper<TRequest, TResponse>>("*", p => new RequestHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    IRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>;

    IRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    IRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(string topicPattern, RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new RequestExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    IRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    IRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        => WithFilter<RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(topicPattern, p => new RequestExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(p, exceptionHandlerFactory));

    IRequestHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IRequestExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
}
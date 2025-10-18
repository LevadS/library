using LevadS.Classes;
using LevadS.Delegates;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

public interface IStreamHandlerBuilder<TRequest, TResponse> : ILevadSBuilder
    where TRequest : IRequest<TResponse>
{
    IStreamHandlerBuilder<TRequest, TResponse> WithFilter(string topicPattern, StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<StreamHandlingFilterWrapper<TRequest, TResponse>>(topicPattern, p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    IStreamHandlerBuilder<TRequest, TResponse> WithFilter(StreamHandlingFilterDelegate<TRequest, TResponse> filterDelegate)
        => WithFilter<StreamHandlingFilterWrapper<TRequest, TResponse>>("*", p => new StreamHandlingFilterWrapper<TRequest, TResponse>(filterDelegate));
    
    IStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern,
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>;

    IStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(
        Func<IServiceProvider, TFilter>? filterFactory = null)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
        => WithFilter<TFilter>("*", filterFactory);
    
    
    
    IStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(string topicPattern, StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithFilter<StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>>(topicPattern, p => new StreamExceptionHandlerDelegateWrapper<TRequest, TResponse, TException>(exceptionHandlerDelegate));
    
    IStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException>(StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
        where TException : Exception
        => WithExceptionHandler("*", exceptionHandlerDelegate);
    
    IStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(string topicPattern,
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TException : Exception
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        => WithFilter<StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>>(topicPattern, p => new StreamExceptionHandlerWrapper<TRequest, TResponse, TException, TExceptionHandler>(p, exceptionHandlerFactory));

    IStreamHandlerBuilder<TRequest, TResponse> WithExceptionHandler<TException, TExceptionHandler>(
        Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory = null)
        where TExceptionHandler : class, IStreamExceptionHandler<TRequest, TResponse, TException>
        where TException : Exception
        => WithExceptionHandler<TException, TExceptionHandler>("*", exceptionHandlerFactory);
}
using Microsoft.Extensions.DependencyInjection;
using LevadS.Services;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;
using LevadS.Delegates;

namespace LevadS.Classes.Builders;

// internal class ServicesRegister(IServiceRegister serviceRegister, IDispatcher dispatcher)
//     : LevadSBuilder2(serviceRegister),
//       IHandlersRegister,
//       IFiltersRegister,
//       IDispatchFiltersRegister,
//       IExceptionHandlersRegister,
//       IMessageServicesRegister,
//       IRequestServicesRegister,
//       IStreamServicesRegister
// {
//     private Dispatcher Dispatcher => (Dispatcher)dispatcher;
//
//     IDisposableMessageHandlerBuilder<TMessage> IMessageHandlersRegister.AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IMessageContext<TMessage>, THandler>? handlerFactory)
//         => (IDisposableMessageHandlerBuilder<TMessage>)AddMessageHandler(topicPattern, handlerFactory);
//
//     IDisposableRequestHandlerBuilder<TRequest, TResponse> IRequestHandlersRegister.AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IRequestContext<TRequest>, THandler>? handlerFactory)
//         => (IDisposableRequestHandlerBuilder<TRequest, TResponse>)AddRequestHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);
//
//     IDisposableStreamHandlerBuilder<TRequest, TResponse> IStreamHandlersRegister.AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IStreamContext<TRequest>, THandler>? handlerFactory)
//         => (IDisposableStreamHandlerBuilder<TRequest, TResponse>)AddStreamHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);
//
//     // IAsyncDisposable IMessageFiltersRegister.AddMessageFilter<TMessage, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddMessageFilter<TMessage, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestFiltersRegister.AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddRequestFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestFiltersRegister.AddRequestFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddRequestFilter<TRequest, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamFiltersRegister.AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddStreamFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamFiltersRegister.AddStreamFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddStreamFilter<TRequest, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IMessageDispatchFiltersRegister.AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddMessageDispatchFilter<TMessage, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestDispatchFiltersRegister.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddRequestDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestDispatchFiltersRegister.AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddRequestDispatchFilter<TRequest, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamDispatchFiltersRegister.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddStreamDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamDispatchFiltersRegister.AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     // {
//     //     AddStreamDispatchFilter<TRequest, TFilter>(topicPattern, filterFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IMessageExceptionHandlersRegister.AddMessageExceptionHandler<TMessage, TException>(string topicPattern,
//     //     MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
//     // {
//     //     ((ILevadSBuilder)this).AddMessageExceptionHandler<TMessage, TException>(topicPattern, exceptionHandlerDelegate);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IMessageExceptionHandlersRegister.AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
//     //     Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     // {
//     //     ((ILevadSBuilder)this).AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(topicPattern, exceptionHandlerFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestExceptionHandlersRegister.AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
//     //     RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
//     // {
//     //     ((ILevadSBuilder)this).AddRequestExceptionHandler<TRequest, TResponse, TException>(topicPattern, exceptionHandlerDelegate);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestExceptionHandlersRegister.AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
//     //     Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     // {
//     //     ((ILevadSBuilder)this).AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(topicPattern, exceptionHandlerFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IRequestExceptionHandlersRegister.AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
//     //     Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     // {
//     //     ((ILevadSBuilder)this).AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(topicPattern, exceptionHandlerFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamExceptionHandlersRegister.AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
//     //     StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
//     // {
//     //     ((ILevadSBuilder)this).AddStreamExceptionHandler<TRequest, TResponse, TException>(topicPattern, exceptionHandlerDelegate);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamExceptionHandlersRegister.AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
//     //     Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     // {
//     //     ((ILevadSBuilder)this).AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(topicPattern, exceptionHandlerFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     //
//     // IAsyncDisposable IStreamExceptionHandlersRegister.AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
//     //     Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     // {
//     //     ((ILevadSBuilder)this).AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(topicPattern, exceptionHandlerFactory);
//     //     Dispatcher.UpdateServices(ServiceCollection);
//     //     return new HandlerDescriptor(Dispatcher, ServiceCollection.TakeLast(1));
//     // }
//     IAsyncDisposable IMessageFiltersRegister.AddMessageFilter<TMessage, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestFiltersRegister.AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestFiltersRegister.AddRequestFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamFiltersRegister.AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamFiltersRegister.AddStreamFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IMessageDispatchFiltersRegister.AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestDispatchFiltersRegister.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestDispatchFiltersRegister.AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamDispatchFiltersRegister.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamDispatchFiltersRegister.AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IMessageExceptionHandlersRegister.AddMessageExceptionHandler<TMessage, TException>(string topicPattern,
//         MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandlerDelegate)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IMessageExceptionHandlersRegister.AddMessageExceptionHandler<TMessage, TException, TExceptionHandler>(string topicPattern,
//         Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestExceptionHandlersRegister.AddRequestExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
//         RequestExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestExceptionHandlersRegister.AddRequestExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
//         Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IRequestExceptionHandlersRegister.AddRequestExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
//         Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamExceptionHandlersRegister.AddStreamExceptionHandler<TRequest, TResponse, TException>(string topicPattern,
//         StreamExceptionHandlerDelegate<TRequest, TResponse, TException> exceptionHandlerDelegate)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamExceptionHandlersRegister.AddStreamExceptionHandler<TRequest, TResponse, TException, TExceptionHandler>(string topicPattern,
//         Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     {
//         throw new NotImplementedException();
//     }
//
//     IAsyncDisposable IStreamExceptionHandlersRegister.AddStreamExceptionHandler<TRequest, TException, TExceptionHandler>(string topicPattern,
//         Func<IServiceProvider, TExceptionHandler>? exceptionHandlerFactory)
//     {
//         throw new NotImplementedException();
//     }
// }

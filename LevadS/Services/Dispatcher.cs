using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LevadS.Enums;
using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace LevadS.Services;

internal class Dispatcher : IDispatcher, IServiceManager
{
    public Dispatcher(IServiceCollection serviceCollection, IServiceProvider serviceProvider)
    {
        ServiceCollection = serviceCollection.Clone();
        ServiceCollection.RemoveAll<IDispatcher>();
        ServiceCollection.AddSingleton<IDispatcher>(this);
        ServiceProvider = serviceProvider;
    }
    
    private readonly object _lock = new();
    public IServiceCollection ServiceCollection { get; }
    private IServiceProvider ServiceProvider { get; set; }

    public void UpdateServices(IServiceCollection newServiceCollection)
    {
        lock (_lock)
        {
            ServiceCollection.Add(newServiceCollection.Except(ServiceCollection));
            ServiceProvider = ServiceCollection.BuildServiceProvider();
        }
    }
    
    public void RemoveServices(IEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        lock (_lock)
        {
            foreach (var serviceDescriptor in serviceDescriptors)
            {
                ServiceCollection.Remove(serviceDescriptor);
            }
            
            ServiceProvider = ServiceCollection.BuildServiceProvider();
        }
    }
    
    private Task InternalUnicastAsync<TMessage>(TMessage message, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new MessageContext<TMessage>(ServiceProvider)
        {
            Message = message,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Send,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceProvider
        };

        return InternalDispatchMessage(context, async () =>
        {
            var messageHandler = ServiceProvider.GetTopicMessageHandler(context);
            if (messageHandler != null)
            {
                await InternalHandleMessageAsync(messageHandler);
            }
        }, context.CancellationToken);
    }
    
    private Task InternalMulticastAsync<TMessage>(TMessage message, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new MessageContext<TMessage>(ServiceProvider)
        {
            Message = message,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Publish,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceProvider
        };

        return InternalDispatchMessage(context, async () =>
        {
            var handlers = ServiceProvider.GetTopicMessageHandlers(context);
            foreach (var handler in handlers)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                await InternalHandleMessageAsync(handler, cancellationToken);
            }
        });
    }

    private async Task InternalDispatchMessage<TMessage>(MessageContext<TMessage> context, MessageHandlingFilterNextDelegate handler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceProvider.GetTopicMessageDispatchFilters(context).ToList();
        using var enumerator = filters.GetEnumerator();
        MessageDispatchFilterNextDelegate callback = null!;
        callback = async (topic, headers) =>
        {
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var dispatchContext = new MessageContext<TMessage>(ServiceProvider)
                {
                    Message = context.Message,
                    Topic = topic ?? context.Topic,
                    Headers = new Dictionary<string, object>(headers ?? context.Headers),
                    DispatchType = context.DispatchType,
                    CapturedTopicValues = enumerator.Current.Context!.CapturedTopicValues,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };
                
                await enumerator.Current.InvokeAsync(dispatchContext, callback);
            }
            else
            {
                context.Topic = topic ?? context.Topic;
                context.Headers = headers ?? context.Headers;
                
                await handler();
            }
        };

        await callback(context.Topic, context.Headers);
    }

    private async Task InternalHandleMessageAsync<TMessage>(ITopicMessageHandler<TMessage> messageHandler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceProvider.GetTopicMessageHandlingFilters((MessageContext<TMessage>)messageHandler.Context!, messageHandler.Key).ToList();
        using var enumerator = filters.GetEnumerator();
        MessageHandlingFilterNextDelegate callback = null!;
        callback = async () =>
        {
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var context = new MessageContext<TMessage>(ServiceProvider)
                {
                    Message = ((MessageContext<TMessage>)messageHandler.Context!).Message,
                    Topic = messageHandler.Context.Topic,
                    Headers = new Dictionary<string, object>(messageHandler.Context.Headers),
                    DispatchType = messageHandler.Context.DispatchType,
                    CapturedTopicValues = enumerator.Current.Context!.CapturedTopicValues,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };

                await enumerator.Current.InvokeAsync(context, callback);
            }
            else
            {
                var context = new MessageContext<TMessage>(ServiceProvider)
                {
                    Message = ((MessageContext<TMessage>)messageHandler.Context!).Message,
                    Topic = messageHandler.Context.Topic,
                    Headers = new Dictionary<string, object>(messageHandler.Context.Headers),
                    DispatchType = messageHandler.Context.DispatchType,
                    CapturedTopicValues = new Dictionary<string, object>(messageHandler.Context!.CapturedTopicValues),
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };

                await messageHandler.HandleAsync(context);
            }
        };

        await callback();
    }

    private Task<TResponse> InternalDispatchRequest<TRequest, TResponse>(RequestContext<TRequest> context, RequestHandlingFilterNextDelegate<TResponse> handler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceProvider.GetTopicRequestDispatchFilters<TRequest, TResponse>(context).ToList();
        using var enumerator = filters.GetEnumerator();
        RequestDispatchFilterNextDelegate<TResponse> callback = null!;
        callback = (topic, headers) =>
        {
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var dispatchContext = new RequestContext<TRequest>(ServiceProvider)
                {
                    Request = context.Request,
                    Topic = topic ?? context.Topic,
                    Headers = new Dictionary<string, object>(headers ?? context.Headers),
                    DispatchType = context.DispatchType,
                    CapturedTopicValues = enumerator.Current.Context!.CapturedTopicValues,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };

                return enumerator.Current.InvokeAsync(dispatchContext, callback);
            }
            
            context.Topic = topic ?? context.Topic;
            context.Headers = headers ?? context.Headers;

            return handler();
        };

        return callback(context.Topic, context.Headers);
    }

    private async IAsyncEnumerable<TResponse> InternalDispatchStream<TRequest, TResponse>(StreamContext<TRequest> context,
        StreamHandlingFilterNextDelegate<TResponse> handler, CancellationToken? cancellationToken)
    {
        var filters = ServiceProvider.GetTopicStreamDispatchFilters<TRequest, TResponse>(context).ToList();
        using var enumerator = filters.GetEnumerator();
        StreamDispatchFilterNextDelegate<TResponse> callback = null!;
        callback = (topic, headers) =>
        {
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var dispatchContext = new StreamContext<TRequest>(ServiceProvider)
                {
                    Request = context.Request,
                    Topic = topic ?? context.Topic,
                    Headers = new Dictionary<string, object>(headers ?? context.Headers),
                    DispatchType = context.DispatchType,
                    CapturedTopicValues = enumerator.Current.Context!.CapturedTopicValues,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };
                
                return enumerator.Current.InvokeAsync(dispatchContext, callback);
            }
            
            context.Topic = topic ?? context.Topic;
            context.Headers = headers ?? context.Headers;

            return handler();
        };

        await foreach (var response in callback(context.Topic, context.Headers))
        {
            yield return response;
        }
    }
    
    private Task<TResponse> InternalRequestAsync<TRequest, TResponse>(TRequest request, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new RequestContext<TRequest>(ServiceProvider)
        {
            Request = request,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Request,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceProvider
        };

        return InternalDispatchRequest(context, async () =>
        {
            var requestHandler = ServiceProvider.GetTopicRequestHandler<TRequest, TResponse>(context);
            if (requestHandler != null)
            {
                return await InternalHandleRequest(requestHandler);
            }

            throw new InvalidOperationException($"No handler found for {typeof(TRequest).Name}");
        }, context.CancellationToken);
    }
    
    private IAsyncEnumerable<TResponse> InternalStreamAsync<TRequest, TResponse>(TRequest request, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new StreamContext<TRequest>(ServiceProvider)
        {
            Request = request,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Stream,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceProvider
        };

        return InternalDispatchStream(context, () =>
        {
            var messageHandler = ServiceProvider.GetTopicStreamHandler<TRequest, TResponse>(context);
            if (messageHandler != null)
            {
                return InternalHandleStream(messageHandler);
            }

            throw new InvalidOperationException($"No handler found for {typeof(TRequest).Name}");
        }, context.CancellationToken);
    }

    private async Task<TResponse> InternalHandleRequest<TRequest, TResponse>(ITopicRequestHandler<TRequest, TResponse> requestHandler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceProvider.GetTopicRequestHandlingFilters<TRequest, TResponse>((RequestContext<TRequest>)requestHandler.Context!, requestHandler.Key).ToList();
        using var enumerator = filters.GetEnumerator();
        RequestHandlingFilterNextDelegate<TResponse> callback = null!;
        callback = async () =>
        {
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var context = new RequestContext<TRequest>(ServiceProvider)
                {
                    Request = ((RequestContext<TRequest>)requestHandler.Context!).Request,
                    Topic = requestHandler.Context.Topic,
                    Headers = new Dictionary<string, object>(requestHandler.Context.Headers),
                    DispatchType = requestHandler.Context.DispatchType,
                    CapturedTopicValues = enumerator.Current.Context!.CapturedTopicValues,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };

                return await enumerator.Current.InvokeAsync(context, callback);
            }
            else
            {
                var context = new RequestContext<TRequest>(ServiceProvider)
                {
                    Request = ((RequestContext<TRequest>)requestHandler.Context).Request,
                    Topic = requestHandler.Context.Topic,
                    Headers = new Dictionary<string, object>(requestHandler.Context.Headers),
                    DispatchType = requestHandler.Context.DispatchType,
                    CapturedTopicValues = new Dictionary<string, object>(requestHandler.Context!.CapturedTopicValues),
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };
                    
                return await requestHandler.HandleAsync(context);
            }
        };

        return await callback();
    }

    private async IAsyncEnumerable<TResponse> InternalHandleStream<TRequest, TResponse>(ITopicStreamHandler<TRequest, TResponse> streamHandler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceProvider.GetTopicStreamHandlingFilters<TRequest, TResponse>((StreamContext<TRequest>)streamHandler.Context!, streamHandler.Key).ToList();
        using var enumerator = filters.GetEnumerator();
     
        StreamHandlingFilterNextDelegate<TResponse> callback = null!;
        callback = () =>
        {
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var context = new StreamContext<TRequest>(ServiceProvider)
                {
                    Request = ((StreamContext<TRequest>)streamHandler.Context!).Request,
                    Topic = streamHandler.Context.Topic,
                    Headers = new Dictionary<string, object>(streamHandler.Context.Headers),
                    DispatchType = streamHandler.Context.DispatchType,
                    CapturedTopicValues = enumerator.Current.Context!.CapturedTopicValues,
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };

                return enumerator.Current.InvokeAsync(context, callback);
            }
            else
            {
                var context = new StreamContext<TRequest>(ServiceProvider)
                {
                    Request = ((StreamContext<TRequest>)streamHandler.Context!).Request,
                    Topic = streamHandler.Context.Topic,
                    Headers = new Dictionary<string, object>(streamHandler.Context.Headers),
                    DispatchType = streamHandler.Context.DispatchType,
                    CapturedTopicValues = new Dictionary<string, object>(streamHandler.Context!.CapturedTopicValues),
                    CancellationToken = cancellationToken ?? CancellationToken.None
                };

                return streamHandler.HandleAsync(context, CancellationToken.None);
            }
        };
        
        await foreach (var response in callback())
        {
            yield return response;
        }
    }

    public Task SendAsync<TMessage>(TMessage message, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
        => InternalUnicastAsync(message, topic, headers, cancellationToken);

    public Task PublishAsync<TMessage>(TMessage message, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
        => InternalMulticastAsync(message, topic, headers, cancellationToken);

    private readonly MethodInfo _internalRequestAsyncMethod = typeof(Dispatcher)
        .GetMethod(nameof(InternalRequestAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;

    public Task<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, string topic = "*",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
        => (Task<TResponse>)_internalRequestAsyncMethod
            .MakeGenericMethod(request.GetType(), typeof(TResponse))
            .Invoke(this, [request, topic, headers, cancellationToken])!;

    public Task<TResponse> RequestAsync<TResponse>(object request, string topic = "",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
        => (Task<TResponse>)_internalRequestAsyncMethod
            .MakeGenericMethod(request.GetType(), typeof(TResponse))
            .Invoke(this, [request, topic, headers, cancellationToken])!;

    private readonly MethodInfo _internalStreamAsyncMethod = typeof(Dispatcher)
        .GetMethod(nameof(InternalStreamAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(IRequest<TResponse> request, string topic = "*",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
        =>  (IAsyncEnumerable<TResponse>)_internalStreamAsyncMethod!
            .MakeGenericMethod(request.GetType(), typeof(TResponse))
            .Invoke(this, [request, topic, headers, cancellationToken])!;

    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(object request, string topic = "",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
        => (IAsyncEnumerable<TResponse>)_internalStreamAsyncMethod!
            .MakeGenericMethod(request.GetType(), typeof(TResponse))
            .Invoke(this, [request, topic, headers, cancellationToken])!;
}
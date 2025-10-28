using System.Reflection;
using LevadS.Enums;
using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Classes.Extensions;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace LevadS.Services;

internal class Dispatcher : IDispatcher
{
    private readonly IServiceResolver ServiceResolver;
    public Dispatcher(IServiceProvider serviceProvider, IServiceResolver serviceResolver)
    {
        ((ServiceContainer)serviceResolver).ServiceProvider = serviceProvider;
        ServiceResolver = serviceResolver;
    }
    
    private Task InternalUnicastAsync<TMessage>(TMessage message, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new MessageContext<TMessage>(ServiceResolver.ServiceProvider)
        {
            Message = message,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Send,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceResolver.ServiceProvider
        };

        return InternalDispatchMessageAsync(context, async () =>
        {
            var messageHandler = ServiceResolver.GetMessageHandler(context);
            if (messageHandler is not { Service: not null })
            {
                throw new InvalidOperationException($"No handler found for {typeof(TMessage).Name}");
            }

            await InternalHandleMessageAsync(messageHandler.Value.Service, messageHandler.Value.Context, messageHandler.Value.Context.CancellationToken);
        }, context.CancellationToken);
    }
    
    private Task InternalMulticastAsync<TMessage>(TMessage message, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new MessageContext<TMessage>(ServiceResolver.ServiceProvider)
        {
            Message = message,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Publish,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceResolver.ServiceProvider
        };

        return InternalDispatchMessageAsync(context, async () =>
        {
            var handlers = ServiceResolver.GetMessageHandlers(context);
            foreach (var handler in handlers)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                
                await InternalHandleMessageAsync(handler.Service, handler.Context, cancellationToken);
            }
        });
    }

    private Task InternalDispatchMessageAsync<TMessage>(MessageContext<TMessage> context, MessageHandlingFilterNextDelegate handler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceResolver.GetMessageDispatchFilters(context).ToList();
        using var enumerator = filters.GetEnumerator();
        MessageDispatchFilterNextDelegate callback = null!;
        callback = async (topic, headers) =>
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            if (enumerator.MoveNext() && enumerator.Current.Service != null)
            {
                await enumerator.Current.Service.InvokeAsync(enumerator.Current.Context, callback);
            }
            else
            {
                context.Topic = topic ?? context.Topic;
                context.Headers = headers ?? context.Headers;
                
                await handler();
            }
        };

        return callback(context.Topic, context.Headers);
    }

    private Task InternalHandleMessageAsync<TMessage>(IMessageHandler<TMessage> messageHandler, IMessageContext<TMessage> context, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceResolver.GetMessageHandlingFilters(context, ((Context)context).Key!).ToList();
        using var enumerator = filters.GetEnumerator();
        MessageHandlingFilterNextDelegate callback = null!;
        callback = async () =>
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (enumerator.MoveNext() && enumerator.Current.Service != null)
            {
                await enumerator.Current.Service.InvokeAsync(enumerator.Current.Context, callback);
            }
            else
            {
                await messageHandler.HandleAsync(context);
            }
        };
        
        return callback();
    }

    private Task<TResponse> InternalDispatchRequestAsync<TRequest, TResponse>(RequestContext<TRequest> context, RequestHandlingFilterNextDelegate<TResponse> handler, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceResolver.GetRequestDispatchFilters<TRequest, TResponse>(context).ToList();
        using var enumerator = filters.GetEnumerator();
        RequestDispatchFilterNextDelegate<TResponse> callback = null!;
        callback = (topic, headers) =>
        {
            if (enumerator.MoveNext() && enumerator.Current.Service != null)
            {
                return enumerator.Current.Service.InvokeAsync(enumerator.Current.Context, callback);
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
        var filters = ServiceResolver.GetStreamDispatchFilters<TRequest, TResponse>(context).ToList();
        using var enumerator = filters.GetEnumerator();
        StreamDispatchFilterNextDelegate<TResponse> callback = null!;
        callback = (topic, headers) =>
        {
            if (enumerator.MoveNext() && enumerator.Current.Service != null)
            {
                return enumerator.Current.Service.InvokeAsync(enumerator.Current.Context, callback);
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
        var context = new RequestContext<TRequest>(ServiceResolver.ServiceProvider)
        {
            Request = request,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Request,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceResolver.ServiceProvider
        };

        return InternalDispatchRequestAsync<TRequest, TResponse>(context, async () =>
        {
            var requestHandler = ServiceResolver.GetRequestHandler<TRequest, TResponse>(context);
            if (requestHandler is { Service: not null })
            {
                return await InternalHandleRequest(requestHandler.Value.Service, requestHandler.Value.Context);
            }

            throw new InvalidOperationException($"No handler found for {typeof(TRequest).Name}");
        }, context.CancellationToken);
    }
    
    private IAsyncEnumerable<TResponse> InternalStreamAsync<TRequest, TResponse>(TRequest request, string topic = "*", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        var context = new StreamContext<TRequest>(ServiceResolver.ServiceProvider)
        {
            Request = request,
            Topic = topic,
            Headers = headers ?? [],
            DispatchType = DispatchType.Stream,
            CancellationToken = cancellationToken ?? CancellationToken.None,
            ServiceProvider = ServiceResolver.ServiceProvider
        };

        return InternalDispatchStream(context, () =>
        {
            var messageHandler = ServiceResolver.GetStreamHandler<TRequest, TResponse>(context);
            if (messageHandler is { Service: not null })
            {
                return InternalHandleStream(messageHandler.Value.Service, messageHandler.Value.Context);
            }

            throw new InvalidOperationException($"No handler found for {typeof(TRequest).Name}");
        }, context.CancellationToken);
    }

    private async Task<TResponse> InternalHandleRequest<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler, IRequestContext<TRequest> context, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceResolver.GetRequestHandlingFilters<TRequest, TResponse>(context, ((Context)context).Key!).ToList();
        using var enumerator = filters.GetEnumerator();
        RequestHandlingFilterNextDelegate<TResponse> callback = null!;
        callback = async () =>
        {
            if (enumerator.MoveNext() && enumerator.Current.Service != null)
            {
                return await enumerator.Current.Service.InvokeAsync(enumerator.Current.Context, callback);
            }
            else
            {
                return await requestHandler.HandleAsync(context);
            }
        };

        return await callback();
    }

    private async IAsyncEnumerable<TResponse> InternalHandleStream<TRequest, TResponse>(IStreamHandler<TRequest, TResponse> streamHandler, IStreamContext<TRequest> context, CancellationToken? cancellationToken = null)
    {
        var filters = ServiceResolver.GetStreamHandlingFilters<TRequest, TResponse>(context, ((Context)context).Key!).ToList();
        using var enumerator = filters.GetEnumerator();
     
        StreamHandlingFilterNextDelegate<TResponse> callback = null!;
        callback = () =>
        {
            if (enumerator.MoveNext() && enumerator.Current.Service != null)
            {
                return enumerator.Current.Service.InvokeAsync(enumerator.Current.Context, callback);
            }
            else
            {
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
    {
        try
        {
            return (Task<TResponse>)_internalRequestAsyncMethod
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(this, [request, topic, headers, cancellationToken])!;
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException!;
        }
    }

    public Task<TResponse> RequestAsync<TResponse>(object request, string topic = "",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        // dynamic requestObject = request as dynamic;
        // return InternalRequestAsync<dynamic, TResponse>(requestObject, topic, headers, cancellationToken);
        try
        {
            return (Task<TResponse>)_internalRequestAsyncMethod
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(this, [request, topic, headers, cancellationToken])!;
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException!;
        }
    }

    private readonly MethodInfo _internalStreamAsyncMethod = typeof(Dispatcher)
        .GetMethod(nameof(InternalStreamAsync), BindingFlags.Instance | BindingFlags.NonPublic)!;
    
    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(IRequest<TResponse> request, string topic = "*",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        // dynamic requestObject = request as dynamic;
        // return InternalRequestAsync(requestObject, topic, headers, cancellationToken);
        try
        {
            return (IAsyncEnumerable<TResponse>)_internalStreamAsyncMethod!
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(this, [request, topic, headers, cancellationToken])!;
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException!;
        }
    }

    public IAsyncEnumerable<TResponse> StreamAsync<TResponse>(object request, string topic = "",
        Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null)
    {
        // dynamic requestObject = request as dynamic;
        // return InternalStreamAsync(requestObject, topic, headers, cancellationToken);
        try
        {
            return (IAsyncEnumerable<TResponse>)_internalStreamAsyncMethod!
                .MakeGenericMethod(request.GetType(), typeof(TResponse))
                .Invoke(this, [request, topic, headers, cancellationToken])!;
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException!;
        }
    }
}
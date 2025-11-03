using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LevadS;
using LevadS.Classes;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Tests.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevadS.Tests.Tests;

[TestClass]
public class TopicWrappersTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // No LevadS runtime registrations needed for wrapper tests
    }

    [TestMethod]
    public async Task TopicMessageHandler_invokes_inner_handler()
    {
        var called = false;
        var handler = new InlineMessageHandler(() => called = true);
        var wrapper = new TopicMessageHandler<SimpleMessage>(ServiceProvider, _ => handler, "orders:*", key: "k");
        var ctx = new MessageContext<SimpleMessage>(ServiceProvider)
        {
            Message = new SimpleMessage(),
            Topic = "orders:created"
        };

        await wrapper.HandleAsync(ctx);

        Assert.IsTrue(called);
    }

    [TestMethod]
    public async Task TopicRequestHandler_invokes_inner_handler_and_returns_response()
    {
        var wrapper = new TopicRequestHandler<SimpleRequest, SimpleResponse>(ServiceProvider, _ => new InlineRequestHandler(() => new SimpleResponse()), "orders:*", key: "k");
        var ctx = new RequestContext<SimpleRequest>(ServiceProvider)
        {
            Request = new SimpleRequest(),
            Topic = "orders:get"
        };

        var resp = await wrapper.HandleAsync(ctx);
        Assert.IsNotNull(resp);
    }

    [TestMethod]
    public async Task TopicStreamHandler_invokes_inner_handler_and_yields_items()
    {
        var items = new[] { new SimpleResponse(), new SimpleResponse() };
        var wrapper = new TopicStreamHandler<SimpleRequest, SimpleResponse>(ServiceProvider, _ => new InlineStreamHandler(items), "orders:*", key: "k");
        var ctx = new StreamContext<SimpleRequest>(ServiceProvider)
        {
            Request = new SimpleRequest(),
            Topic = "orders:stream"
        };

        var collected = new List<SimpleResponse>();
        await foreach (var x in wrapper.HandleAsync(ctx, CancellationToken.None))
        {
            collected.Add(x);
        }

        Assert.AreEqual(2, collected.Count);
    }

    [TestMethod]
    public async Task TopicMessageDispatchFilter_forwards_to_inner_and_next()
    {
        var innerCalled = false;
        var nextCalled = false;
        var wrapper = new TopicMessageDispatchFilter<SimpleMessage>(ServiceProvider, _ => new InlineMessageDispatchFilter((_, _) => { innerCalled = true; nextCalled = true; return Task.CompletedTask; }), "orders:*");
        var ctx = new MessageContext<SimpleMessage>(ServiceProvider) { Message = new SimpleMessage(), Topic = "orders:created" };

        await wrapper.InvokeAsync(ctx, (t, h) => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(innerCalled);
        Assert.IsTrue(nextCalled);
    }

    [TestMethod]
    public async Task TopicMessageHandlingFilter_forwards_to_inner_and_next()
    {
        var innerCalled = false;
        var nextCalled = false;
        var wrapper = new TopicMessageHandlingFilter<SimpleMessage>(ServiceProvider, _ => new InlineMessageHandlingFilter(h => { innerCalled = true; nextCalled = true; return Task.CompletedTask; }), "orders:*", key: "k");
        var ctx = new MessageContext<SimpleMessage>(ServiceProvider) { Message = new SimpleMessage(), Topic = "orders:created" };

        await wrapper.InvokeAsync(ctx, h => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(innerCalled);
        Assert.IsTrue(nextCalled);
    }

    [TestMethod]
    public async Task TopicRequestDispatchFilter_forwards_to_inner_and_next()
    {
        var innerCalled = false;
        var wrapper = new TopicRequestDispatchFilter<SimpleRequest, SimpleResponse>(ServiceProvider, _ => new InlineRequestDispatchFilter<SimpleRequest, SimpleResponse>((_, next) => { innerCalled = true; return next("orders:ok", null!); }), "orders:*");
        var ctx = new RequestContext<SimpleRequest>(ServiceProvider) { Request = new SimpleRequest(), Topic = "orders:created" };

        var resp = await wrapper.InvokeAsync(ctx, (t, h) => Task.FromResult(new SimpleResponse()));
        Assert.IsTrue(innerCalled);
        Assert.IsNotNull(resp);
    }

    [TestMethod]
    public async Task TopicRequestHandlingFilter_forwards_to_inner_and_next()
    {
        var innerCalled = false;
        var wrapper = new TopicRequestHandlingFilter<SimpleRequest, SimpleResponse>(ServiceProvider, _ => new InlineRequestHandlingFilter<SimpleRequest, SimpleResponse>((_, next) => { innerCalled = true; return next(null!); }), "orders:*", key: "k");
        var ctx = new RequestContext<SimpleRequest>(ServiceProvider) { Request = new SimpleRequest(), Topic = "orders:created" };

        var resp = await wrapper.InvokeAsync(ctx, _ => Task.FromResult(new SimpleResponse()));
        Assert.IsTrue(innerCalled);
        Assert.IsNotNull(resp);
    }

    [TestMethod]
    public async Task TopicStreamDispatchFilter_forwards_to_inner_and_next()
    {
        var innerCalled = false;
        var items = new[] { new SimpleResponse() };
        var wrapper = new TopicStreamDispatchFilter<SimpleRequest, SimpleResponse>(ServiceProvider, _ => new InlineStreamDispatchFilter<SimpleRequest, SimpleResponse>((_, next) => { innerCalled = true; return next("orders:stream", null); }), "orders:*");
        var ctx = new StreamContext<SimpleRequest>(ServiceProvider) { Request = new SimpleRequest(), Topic = "orders:stream" };

        var collected = new List<SimpleResponse>();
        await foreach (var x in wrapper.InvokeAsync(ctx, (t, h) => items.ToAsyncEnumerable()))
        {
            collected.Add(x);
        }

        Assert.IsTrue(innerCalled);
        Assert.AreEqual(1, collected.Count);
    }

    [TestMethod]
    public async Task TopicStreamHandlingFilter_forwards_to_inner_and_next()
    {
        var innerCalled = false;
        var items = new[] { new SimpleResponse(), new SimpleResponse() };
        var wrapper = new TopicStreamHandlingFilter<SimpleRequest, SimpleResponse>(ServiceProvider, _ => new InlineStreamHandlingFilter<SimpleRequest, SimpleResponse>((_, next) => { innerCalled = true; return next(null!); }), "orders:*", key: "k");
        var ctx = new StreamContext<SimpleRequest>(ServiceProvider) { Request = new SimpleRequest(), Topic = "orders:stream" };

        var collected = new List<SimpleResponse>();
        await foreach (var x in wrapper.InvokeAsync(ctx, _ => items.ToAsyncEnumerable()))
        {
            collected.Add(x);
        }

        Assert.IsTrue(innerCalled);
        Assert.AreEqual(2, collected.Count);
    }

    private sealed class InlineMessageHandler : IMessageHandler<SimpleMessage>
    {
        private readonly Action _onCall;
        public InlineMessageHandler(Action onCall) => _onCall = onCall;
        public Task HandleAsync(IMessageContext<SimpleMessage> ctx) { _onCall(); return Task.CompletedTask; }
    }

    private sealed class InlineRequestHandler : IRequestHandler<SimpleRequest, SimpleResponse>
    {
        private readonly Func<SimpleResponse> _factory;
        public InlineRequestHandler(Func<SimpleResponse> factory) => _factory = factory;
        public Task<SimpleResponse> HandleAsync(IRequestContext<SimpleRequest> ctx) => Task.FromResult(_factory());
    }

    private sealed class InlineStreamHandler : IStreamHandler<SimpleRequest, SimpleResponse>
    {
        private readonly IEnumerable<SimpleResponse> _items;
        public InlineStreamHandler(IEnumerable<SimpleResponse> items) => _items = items;
        public async IAsyncEnumerable<SimpleResponse> HandleAsync(IStreamContext<SimpleRequest> ctx, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var i in _items) { yield return i; await Task.Yield(); }
        }
    }

    private sealed class InlineMessageDispatchFilter : IMessageDispatchFilter<SimpleMessage>
    {
        private readonly Func<string?, Dictionary<string, object>?, Task> _impl;
        public InlineMessageDispatchFilter(Func<string?, Dictionary<string, object>?, Task> impl) => _impl = impl;
        public Task InvokeAsync(IMessageContext<SimpleMessage> ctx, MessageDispatchFilterNextDelegate next) => _impl(ctx.Topic, ctx.Headers.ToDictionary());
    }

    private sealed class InlineMessageHandlingFilter : IMessageHandlingFilter<SimpleMessage>
    {
        private readonly Func<Dictionary<string, object>?, Task> _impl;
        public InlineMessageHandlingFilter(Func<Dictionary<string, object>?, Task> impl) => _impl = impl;
        public Task InvokeAsync(IMessageContext<SimpleMessage> ctx, MessageHandlingFilterNextDelegate next) => _impl(ctx.Headers.ToDictionary());
    }

    private sealed class InlineRequestDispatchFilter<TRequest, TResponse> : IRequestDispatchFilter<TRequest, TResponse>
    {
        private readonly Func<IRequestContext<TRequest>, RequestDispatchFilterNextDelegate<TResponse>, Task<TResponse>> _impl;
        public InlineRequestDispatchFilter(Func<IRequestContext<TRequest>, RequestDispatchFilterNextDelegate<TResponse>, Task<TResponse>> impl) => _impl = impl;
        public Task<TResponse> InvokeAsync(IRequestContext<TRequest> ctx, RequestDispatchFilterNextDelegate<TResponse> next) => _impl(ctx, next);
    }

    private sealed class InlineRequestHandlingFilter<TRequest, TResponse> : IRequestHandlingFilter<TRequest, TResponse>
    {
        private readonly Func<IRequestContext<TRequest>, RequestHandlingFilterNextDelegate<TResponse>, Task<TResponse>> _impl;
        public InlineRequestHandlingFilter(Func<IRequestContext<TRequest>, RequestHandlingFilterNextDelegate<TResponse>, Task<TResponse>> impl) => _impl = impl;
        public Task<TResponse> InvokeAsync(IRequestContext<TRequest> ctx, RequestHandlingFilterNextDelegate<TResponse> next) => _impl(ctx, next);
    }

    private sealed class InlineStreamDispatchFilter<TRequest, TResponse> : IStreamDispatchFilter<TRequest, TResponse>
    {
        private readonly Func<IStreamContext<TRequest>, StreamDispatchFilterNextDelegate<TResponse>, IAsyncEnumerable<TResponse>> _impl;
        public InlineStreamDispatchFilter(Func<IStreamContext<TRequest>, StreamDispatchFilterNextDelegate<TResponse>, IAsyncEnumerable<TResponse>> impl) => _impl = impl;
        public IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> ctx, StreamDispatchFilterNextDelegate<TResponse> next) => _impl(ctx, next);
    }

    private sealed class InlineStreamHandlingFilter<TRequest, TResponse> : IStreamHandlingFilter<TRequest, TResponse>
    {
        private readonly Func<IStreamContext<TRequest>, StreamHandlingFilterNextDelegate<TResponse>, IAsyncEnumerable<TResponse>> _impl;
        public InlineStreamHandlingFilter(Func<IStreamContext<TRequest>, StreamHandlingFilterNextDelegate<TResponse>, IAsyncEnumerable<TResponse>> impl) => _impl = impl;
        public IAsyncEnumerable<TResponse> InvokeAsync(IStreamContext<TRequest> ctx, StreamHandlingFilterNextDelegate<TResponse> next) => _impl(ctx, next);
    }
}

// Minimal local async enumerable helper so we don't need extra packages
public static class EnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> items)
    {
        foreach (var i in items)
        {
            yield return i;
            await Task.Yield();
        }
    }
}
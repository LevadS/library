using LevadS.Delegates;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class DisposableHandlersTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.EnableRuntimeRegistrations();
        
        // No static handlers; we will register and dispose dynamically via IHandlersRegister
    }

    private sealed class Msg { }
    private sealed class Req : IRequest<int> { }
    private sealed class SReq : IRequest<int> { }

    [TestMethod]
    public async Task Disposable_Message_Handler_And_ScopedFilters()
    {
        var handled = 0;
        var scoped = 0;

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        var builder = reg.AddMessageHandler<Msg>("disposable", () => { handled++; return Task.CompletedTask; })
            .WithFilter("disposable", (ctx, next) => { scoped++; return next(); });

        var handle = builder.Build();
        try
        {
            await Dispatcher.SendAsync(new Msg(), "disposable");
            Assert.AreEqual(1, handled);
            Assert.AreEqual(1, scoped);

            // After disposing, sending again should not invoke handler nor scoped filters
            await handle.DisposeAsync();
            await Dispatcher.SendAsync(new Msg(), "disposable");
            Assert.AreEqual(1, handled);
            Assert.AreEqual(1, scoped);
        }
        finally
        {
            await handle.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task Disposable_Request_Handler_And_ScopedFilters()
    {
        var handled = 0;
        var scoped = 0;

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        var builder = reg.AddRequestHandler<Req, int>("disposable", () => { handled++; return 7; })
            .WithFilter("disposable", (ctx, next) => { scoped++; return next(); });

        var handle = builder.Build();
        try
        {
            var r = await Dispatcher.RequestAsync(new Req(), "disposable");
            Assert.AreEqual(7, r);
            Assert.AreEqual(1, handled);
            Assert.AreEqual(1, scoped);

            await handle.DisposeAsync();

            // After disposal, request should fail because no handler found
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                _ = await Dispatcher.RequestAsync(new Req(), "disposable");
            });
        }
        finally
        {
            await handle.DisposeAsync();
        }
    }

    private int scoped = 0;

    [TestMethod]
    public async Task Disposable_Stream_Handler_And_ScopedFilters()
    {
        static async IAsyncEnumerable<int> Base()
        {
            yield return 1;
            yield return 2;
            await Task.CompletedTask;
        }

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        var builder = reg.AddStreamHandler<SReq, int>("disposable", Base)
            .WithFilter("disposable", DisposableFilter);

        var handle = builder.Build();
        try
        {
            var list = new List<int>();
            await foreach (var v in Dispatcher.StreamAsync(new SReq(), "disposable")) list.Add(v);
            CollectionAssert.AreEqual(new[] { 1, 2 }, list);
            Assert.AreEqual(1, scoped);

            await handle.DisposeAsync();

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await foreach (var _ in Dispatcher.StreamAsync(new SReq(), "disposable")) { }
            });
        }
        finally
        {
            await handle.DisposeAsync();
        }
    }

    private async IAsyncEnumerable<int> DisposableFilter(IStreamContext<SReq> requestContext, StreamHandlingFilterNextDelegate<int> next)
    {
        scoped++;
        await foreach (var v in next()) yield return v;
    }

    [TestMethod]
    public async Task Disposable_Message_Multicast_DisposeOne_KeepsOther()
    {
        var h1 = 0;
        var h2 = 0;

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        var b1 = reg.AddMessageHandler<Msg>("pub", () => { h1++; return Task.CompletedTask; });
        var b2 = reg.AddMessageHandler<Msg>("pub", () => { h2++; return Task.CompletedTask; });

        var d1 = b1.Build();
        var d2 = b2.Build();
        try
        {
            await Dispatcher.PublishAsync(new Msg(), "pub");
            Assert.AreEqual(1, h1);
            Assert.AreEqual(1, h2);

            await d1.DisposeAsync();

            await Dispatcher.PublishAsync(new Msg(), "pub");
            Assert.AreEqual(1, h1, "disposed handler should not be called again");
            Assert.AreEqual(2, h2, "other handler should still be called");
        }
        finally
        {
            await d1.DisposeAsync();
            await d2.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task Disposable_Stream_Dispose_MidFlight_AllowsCurrent_BlocksNext()
    {
        static async IAsyncEnumerable<int> Slow()
        {
            yield return 1; await Task.Delay(20);
            yield return 2; await Task.Delay(20);
            yield return 3;
        }

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        var builder = reg.AddStreamHandler<SReq, int>("slow", Slow);
        var handle = builder.Build();
        try
        {
            var received = new List<int>();
            var disposed = false;

            await foreach (var v in Dispatcher.StreamAsync(new SReq(), "slow"))
            {
                received.Add(v);
                if (!disposed)
                {
                    disposed = true;
                    await handle.DisposeAsync();
                }
            }

            // Current invocation completes even after disposal
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, received);

            // Subsequent invocation should fail
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await foreach (var _ in Dispatcher.StreamAsync(new SReq(), "slow")) { }
            });
        }
        finally
        {
            await handle.DisposeAsync();
        }
    }
}

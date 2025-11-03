using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LevadS.Interfaces;
using LevadS.Delegates;
using LevadS.Tests.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class DispatcherAdditionalBehaviorTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Empty by default; tests register runtime handlers/filters as needed
    }

    private sealed class CancelMsg { }

    [TestMethod]
    public async Task Send_and_Publish_respect_cancellation_token()
    {
        var sends = 0;
        var pubs = 0;

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        reg.AddMessageHandler<CancelMsg>("cancel-send", () => { sends++; return Task.CompletedTask; }).Build();
        reg.AddMessageHandler<CancelMsg>("cancel-pub", () => { pubs++; return Task.CompletedTask; }).Build();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Dispatcher.SendAsync(new CancelMsg(), "cancel-send", cancellationToken: cts.Token);
        await Dispatcher.PublishAsync(new CancelMsg(), "cancel-pub", cancellationToken: cts.Token);

        Assert.AreEqual(0, sends, "SendAsync should short-circuit when cancelled");
        Assert.AreEqual(0, pubs, "PublishAsync should short-circuit when cancelled");
    }

    private sealed class OkReq : IRequest<int> { }

    [TestMethod]
    public async Task RequestAsync_object_overload_succeeds_when_handler_exists()
    {
        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        reg.AddRequestHandler<OkReq, int>("ok", () => 42).Build();

        var result = await Dispatcher.RequestAsync<int>((object)new OkReq(), "ok");
        Assert.AreEqual(42, result);
    }

    private sealed class OkSReq : IRequest<int> { }

    [TestMethod]
    public async Task StreamAsync_object_overload_succeeds_when_handler_exists()
    {
        static async IAsyncEnumerable<int> Impl()
        {
            yield return 1; await Task.CompletedTask; yield return 2;
        }

        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        reg.AddStreamHandler<OkSReq, int>("ok", Impl).Build();

        var list = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync<int>((object)new OkSReq(), "ok")) list.Add(v);

        CollectionAssert.AreEqual(new[] { 1, 2 }, list);
    }

    private sealed class HdrMsg { }

    [TestMethod]
    public async Task MessageHandling_filter_header_mutation_visible_to_handler()
    {
        var seen = 0;
        var reg = ServiceProvider.GetRequiredService<IHandlersRegister>();
        reg.AddMessageHandler<HdrMsg>("hdr", (IMessageContext<HdrMsg> ctx) => { if (ctx.Headers.TryGetValue("x", out var v) && Equals(v, 2)) seen++; })
           .WithFilter("hdr", (IMessageContext<HdrMsg> ctx, MessageHandlingFilterNextDelegate next) =>
                next(new Dictionary<string, object>(ctx.Headers) { ["x"] = 2 }))
           .Build();

        await Dispatcher.SendAsync(new HdrMsg(), "hdr");
        Assert.AreEqual(1, seen);
    }
}

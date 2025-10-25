using System.Collections.Concurrent;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class MessageFiltersAdvanced : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
    private int _counter;
    private readonly ConcurrentQueue<string> _order = new();
    private readonly List<int> _capturesV = new();
    private readonly List<int> _capturesX = new();
    private bool _thrown;

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Short-circuit setup
        builder.AddMessageHandler<SimpleMessage>("short", () => _counter++);
        builder.AddMessageFilter<SimpleMessage>("short", (ctx, next) => Task.CompletedTask);

        // Topic mismatch filter and matching handler
        builder.AddMessageFilter<SimpleMessage>("only-foo", (ctx, next) => { _counter += 1000; return next(); });
        builder.AddMessageHandler<SimpleMessage>("bar", () => _counter++);

        // Order: global then keyed
        builder
            .AddMessageHandler<SimpleMessage>("ordered", () => Task.CompletedTask)
            .WithFilter("ordered", _ => new RecordingFilter(_order, "K1"));
        builder.AddMessageFilter<SimpleMessage>("ordered", (ctx, next) => { _order.Enqueue("G1"); return next(); });
        builder.AddMessageFilter<SimpleMessage>("ordered", (ctx, next) => { _order.Enqueue("G2"); return next(); });

        // Exception propagation
        builder.AddMessageHandler<SimpleMessage>("throws", () => Task.CompletedTask);
        builder.AddMessageFilter<SimpleMessage>("throws", (ctx, next) => throw new InvalidOperationException("boom"));

        // Captured values isolation
    builder.AddMessageHandler<SimpleMessage>("cap:{v:int}:{x:int}", () => Task.CompletedTask);
    builder.AddMessageFilter<SimpleMessage>("cap:{v:int}:#", (ctx, next) => { _capturesV.Add((int)ctx.CapturedValues["v"]); return next(); });
    builder.AddMessageFilter<SimpleMessage>("cap:#:{x:int}", (ctx, next) => { _capturesX.Add((int)ctx.CapturedValues["x"]); return next(); });
    }

    private sealed class RecordingFilter(ConcurrentQueue<string> order, string marker) : IMessageHandlingFilter<SimpleMessage>
    {
        public Task InvokeAsync(IMessageContext<SimpleMessage> messageContext, MessageHandlingFilterNextDelegate next)
        {
            order.Enqueue(marker);
            return next();
        }
    }

    [TestMethod]
    public async Task MessageFilter_ShortCircuit_SkipsHandler()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "short");
        Assert.AreEqual(0, _counter);
    }

    [TestMethod]
    public async Task MessageFilter_TopicMismatch_DoesNotRunGlobal()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "bar");
        Assert.AreEqual(1, _counter);
    }

    [TestMethod]
    public async Task MessageFilters_Order_GlobalThenScoped()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "ordered");
        var arr = _order.ToArray();
        CollectionAssert.AreEqual(new[] { "G1", "G2", "K1" }, arr);
    }

    [TestMethod]
    public async Task MessageFilters_Exception_BubblesUp()
    {
        try
        {
            await Dispatcher.SendAsync(new SimpleMessage(), "throws");
        }
        catch (InvalidOperationException ex) when (ex.Message == "boom")
        {
            _thrown = true;
        }

        Assert.IsTrue(_thrown);
    }

    [TestMethod]
    public async Task MessageFilters_CapturedValues_IsolatedPerFilter()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "cap:1:40");
        CollectionAssert.AreEqual(new[] { 1 }, _capturesV);
        CollectionAssert.AreEqual(new[] { 40 }, _capturesX);
    }
}

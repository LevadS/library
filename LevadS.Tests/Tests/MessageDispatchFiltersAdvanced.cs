using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class MessageDispatchFiltersAdvanced : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
    private int _handled;

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Header merge & precedence
        builder.AddMessageHandler<SimpleMessage>("hdr", (IMessageContext<SimpleMessage> ctx) => { if (ctx.Headers.TryGetValue("x", out var v) && Equals(v, 2)) _handled++; });
        builder.AddMessageDispatchFilter<SimpleMessage>("hdr", (ctx, next) => next(headers: new Dictionary<string, object>(ctx.Headers) { ["x"] = 1 }));
        builder.AddMessageDispatchFilter<SimpleMessage>("hdr", (ctx, next) => next(headers: new Dictionary<string, object>(ctx.Headers) { ["x"] = 2 }));

        // Short-circuit
        builder.AddMessageHandler<SimpleMessage>("short", () => _handled++);
        builder.AddMessageDispatchFilter<SimpleMessage>("short", (ctx, next) => Task.CompletedTask);
    }

    [TestMethod]
    public async Task MessageDispatch_HeaderMerge_LastWriteWins()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "hdr");
        Assert.AreEqual(1, _handled);
    }

    [TestMethod]
    public async Task MessageDispatch_ShortCircuit_SkipsHandling()
    {
        var before = _handled;
        await Dispatcher.SendAsync(new SimpleMessage(), "short");
        Assert.AreEqual(before, _handled);
    }
}

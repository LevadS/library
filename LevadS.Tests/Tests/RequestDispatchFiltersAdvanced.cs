using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

public class RDReq : IRequest<int> { }

[TestClass]
public class RequestDispatchFiltersAdvanced : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Topic rewrite foo -> bar
        builder.AddRequestHandler<RDReq, int>("bar", () => 7);
        builder.AddRequestDispatchFilter<RDReq, int>("foo", (ctx, next) => next("bar"));

        // Header rewrite visible to handler
        builder.AddRequestHandler<RDReq, int>("hdr", (IRequestContext<RDReq> ctx) => ctx.Headers.ContainsKey("x") ? 1 : 0);
        builder.AddRequestDispatchFilter<RDReq, int>("hdr", (ctx, next) => next(headers: new Dictionary<string, object>(ctx.Headers) { ["x"] = 1 }));

        // Short-circuit return
        builder.AddRequestHandler<RDReq, int>("short", () => 999);
        builder.AddRequestDispatchFilter<RDReq, int>("short", (ctx, next) => Task.FromResult(123));

        // Multiple next calls (should be treated as single-call semantics, last result ignored)
        builder.AddRequestHandler<RDReq, int>("multi", () => 1);
        builder.AddRequestDispatchFilter<RDReq, int>("multi", async (ctx, next) =>
        {
            var a = await next("multi");
            var b = await next("multi");
            return a; // expecting framework to only process first chain; second may run but should not break semantics
        });
    }

    [TestMethod]
    public async Task RequestDispatch_TopicRewrite_ChangesHandler()
    {
        var result = await Dispatcher.RequestAsync(new RDReq(), "foo");
        Assert.AreEqual(7, result);
    }

    [TestMethod]
    public async Task RequestDispatch_HeaderRewrite_VisibleToHandler()
    {
        var result = await Dispatcher.RequestAsync(new RDReq(), "hdr");
        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public async Task RequestDispatch_ShortCircuit_ReturnsValue()
    {
        var result = await Dispatcher.RequestAsync(new RDReq(), "short");
        Assert.AreEqual(123, result);
    }

    [TestMethod]
    public async Task RequestDispatch_MultipleNextCalls_TreatedAsSingleChain()
    {
        var result = await Dispatcher.RequestAsync(new RDReq(), "multi");
        Assert.AreEqual(1, result);
    }
}

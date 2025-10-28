using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

public class MyReq : IRequest<int> { public int Id { get; set; } }

[TestClass]
public class RequestFiltersAdvanced : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
    private int _handlerCalls;

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Discover object-variant filters via assembly registration
        builder.RegisterServicesFromAssemblyContaining<LevadS.Tests.Registrations.Filters.ObjectShortCircuitFilter>();

        // Short-circuit returns value without calling handler
        builder.AddRequestHandler<MyReq, int>("short", () => { _handlerCalls++; return 999; });

        // Object variant conversion path
        builder.AddRequestHandler<MyReq, int>("convert", () => { _handlerCalls++; return 41; });

        // Topic mismatch: filter on foo, handler on bar
        builder.AddRequestFilter<MyReq, int>("foo", async (ctx, next) => { throw new Exception("should not run"); });
        builder.AddRequestHandler<MyReq, int>("bar", () => 10);

        // Captured values isolation
        builder.AddRequestHandler<MyReq, int>("cap:{v:int}:{x:int}", (IRequestContext<MyReq> ctx) => (int)ctx.CapturedValues["v"] + (int)ctx.CapturedValues["x"]);
        builder.AddRequestFilter<MyReq, int>("cap:{v:int}:#", async (ctx, next) => await next() + (int)ctx.CapturedValues["v"]);
        builder.AddRequestFilter<MyReq, int>("cap:#:{x:int}", async (ctx, next) => await next() + (int)ctx.CapturedValues["x"]);
    }

    [TestMethod]
    public async Task RequestFilters_ShortCircuit_ReturnsValue_AndSkipsHandler()
    {
        var result = await Dispatcher.RequestAsync(new MyReq(), "short");
        Assert.AreEqual(123, result);
        Assert.AreEqual(0, _handlerCalls);
    }

    [TestMethod]
    public async Task RequestFilters_ObjectVariant_CanReturnConvertibleType()
    {
        var result = await Dispatcher.RequestAsync(new MyReq(), "convert");
        Assert.AreEqual(42, result);
        Assert.AreEqual(0, _handlerCalls);
    }

    [TestMethod]
    public async Task RequestFilter_TopicMismatch_DoesNotRun()
    {
        var result = await Dispatcher.RequestAsync(new MyReq(), "bar");
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public async Task RequestFilters_CapturedValues_IsolatedPerFilter()
    {
        var result = await Dispatcher.RequestAsync(new MyReq(), "cap:2:3");
        // handler returns v + x = 5; first filter adds v (2) => 7; second adds x (3) => 10
        Assert.AreEqual(10, result);
    }
}

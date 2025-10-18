using LevadS.Delegates;
using LevadS.Interfaces;
using System.Collections.Concurrent;

namespace LevadS.Tests.Tests;

[TestClass]
public class HandlerScopedFiltersTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private int _g;
    private int _k1;
    private int _k2;
    private int _handledA;
    private int _handledB;
    private int _k1Narrow;
    private readonly ConcurrentQueue<string> _order = new();

    private static Task<int> Inc(IRequestContext<Req> ctx, RequestHandlingFilterNextDelegate<int> next, Action hit)
        => Task.Run(async () => { hit(); return await next(); });

    private sealed class Req : IRequest<int> { }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
     var a = builder.AddRequestHandler<Req, int>("scope:a:#", () => { _handledA++; return 1; })
         .WithFilter("scope:a:#", (IServiceProvider p) => new Wrapper<int>((ctx, next) => { _order.Enqueue("K1"); return Inc(ctx, next, () => _k1++); }));
     // Additional narrower scoped filter that should only run for topic "scope:a:only"
     a.WithFilter("scope:a:only", (IServiceProvider p) => new Wrapper<int>((ctx, next) => Inc(ctx, next, () => _k1Narrow++)));

     builder.AddRequestHandler<Req, int>("scope:b", () => { _handledB++; return 2; })
         .WithFilter("scope:b", (IServiceProvider p) => new Wrapper<int>((ctx, next) => { _order.Enqueue("K2"); return Inc(ctx, next, () => _k2++); }));

     builder.AddRequestFilter<Req, int>("scope:*", (ctx, next) => { _g++; _order.Enqueue("G"); return next(); });
    }

    private sealed class Wrapper<T>(RequestHandlingFilterDelegate<Req, T> d) : IRequestHandlingFilter<Req, T>
    {
        public Task<T> InvokeAsync(IRequestContext<Req> requestContext, RequestHandlingFilterNextDelegate<T> next) => d(requestContext, next);
    }

    [TestMethod]
    public async Task ScopedFilters_ApplyOnlyToTheirHandler()
    {
    var a = await Dispatcher.RequestAsync(new Req(), "scope:a:root");
        var b = await Dispatcher.RequestAsync(new Req(), "scope:b");

        Assert.AreEqual(1, a);
        Assert.AreEqual(2, b);
        Assert.AreEqual(2, _g, "global should run twice");
        Assert.AreEqual(1, _k1, "scoped A should run once");
        Assert.AreEqual(1, _k2, "scoped B should run once");
        Assert.AreEqual(1, _handledA);
        Assert.AreEqual(1, _handledB);
    }

    [TestMethod]
    public async Task ScopedFilters_Order_GlobalBeforeScoped()
    {
        _order.Clear();
    var _ = await Dispatcher.RequestAsync(new Req(), "scope:a:root");
        CollectionAssert.AreEqual(new[] { "G", "K1" }, _order.ToArray());
    }

    [TestMethod]
    public async Task ScopedFilters_TopicPatternOnScopedMustMatch()
    {
        // When topic matches the narrower filter, both K1 and K1Narrow should run
        var _ = await Dispatcher.RequestAsync(new Req(), "scope:a:only");
        Assert.AreEqual(1, _k1Narrow);

        // When topic does not match the narrower filter, it should not run
        _ = await Dispatcher.RequestAsync(new Req(), "scope:a:other");
        Assert.AreEqual(1, _k1Narrow, "narrow filter should not run for non-matching topic");
    }
}

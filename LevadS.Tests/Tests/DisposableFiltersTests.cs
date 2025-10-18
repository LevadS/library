using LevadS.Delegates;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class DisposableFiltersTests : BaseTestClass
{
    private int _handledMsg;
    private int _msgFilterHits;
    private int _hdrOk;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private sealed class Msg { }
    private sealed class Rq : IRequest<int> { }
    private sealed class SReq : IRequest<int> { }

    private static async IAsyncEnumerable<int> StreamOne()
    {
        yield return 1;
        yield return 2;
        yield return 3;
    }

    private static async IAsyncEnumerable<int> StreamTwo()
    {
        yield return 10;
        yield return 11;
    }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.EnableRuntimeRegistrations();
        
        // Base handlers we will decorate at runtime with filters
        builder.AddMessageHandler<Msg>("m", () => { _handledMsg++; });

        builder.AddRequestHandler<Rq, int>("r", () => 10);

        builder.AddStreamHandler<SReq, int>("s", StreamOne);

        // For dispatch tests
        builder.AddMessageHandler<Msg>("md", (IMessageContext<Msg> ctx) =>
        {
            if (ctx.Headers.TryGetValue("x", out var v) && Equals(v, 1)) _hdrOk++;
        });

        builder.AddRequestHandler<Rq, int>("rd", () => 7);

        builder.AddStreamHandler<SReq, int>("sd", StreamTwo);
    }

    [TestMethod]
    public async Task Disposable_MessageHandlingFilter_Applies_Then_Removed()
    {
        var filters = ServiceProvider.GetRequiredService<IMessageFiltersRegister>();

        await using (filters.AddMessageFilter<Msg>("m", (ctx, next) => { _msgFilterHits++; return next(); }))
        {
            await Dispatcher.SendAsync(new Msg(), "m");
            Assert.AreEqual(1, _msgFilterHits);
            Assert.AreEqual(1, _handledMsg);
        }

        // After disposal, handler still runs but filter no longer hits
        await Dispatcher.SendAsync(new Msg(), "m");
        Assert.AreEqual(1, _msgFilterHits);
        Assert.AreEqual(2, _handledMsg);
    }

    [TestMethod]
    public async Task Disposable_RequestHandlingFilter_Alters_Response_Then_Removed()
    {
        var filters = ServiceProvider.GetRequiredService<IRequestFiltersRegister>();

        await using (filters.AddRequestFilter<Rq, int>("r", async (ctx, next) => (await next()) + 5))
        {
            var r = await Dispatcher.RequestAsync(new Rq(), "r");
            Assert.AreEqual(15, r);
        }

        var r2 = await Dispatcher.RequestAsync(new Rq(), "r");
        Assert.AreEqual(10, r2);
    }

    [TestMethod]
    public async Task Disposable_StreamHandlingFilter_Transforms_Stream_Then_Removed()
    {
        var filters = ServiceProvider.GetRequiredService<IStreamFiltersRegister>();

        await using (filters.AddStreamFilter<SReq, int>("s", AddOne))
        {
            var list = new List<int>();
            await foreach (var v in Dispatcher.StreamAsync(new SReq(), "s")) list.Add(v);
            CollectionAssert.AreEqual(new[] { 2, 3, 4 }, list);
        }

        var list2 = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync(new SReq(), "s")) list2.Add(v);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list2);
    }

    private async IAsyncEnumerable<int> AddOne(IStreamContext<SReq> ctx, StreamHandlingFilterNextDelegate<int> next)
    {
        await foreach (var v in next()) yield return v + 1;
    }

    [TestMethod]
    public async Task Disposable_MessageDispatchFilter_Applies_Then_Removed()
    {
        var dispatch = ServiceProvider.GetRequiredService<IDispatchFiltersRegister>();

        await using (dispatch.AddMessageDispatchFilter<Msg>("md", (ctx, next) => next(headers: new Dictionary<string, object>(ctx.Headers) { ["x"] = 1 })))
        {
            await Dispatcher.SendAsync(new Msg(), "md");
            Assert.AreEqual(1, _hdrOk);
        }

        await Dispatcher.SendAsync(new Msg(), "md");
        Assert.AreEqual(1, _hdrOk);
    }

    [TestMethod]
    public async Task Disposable_RequestDispatchFilter_ShortCircuits_Then_Removed()
    {
        var dispatch = ServiceProvider.GetRequiredService<IDispatchFiltersRegister>();

        await using (dispatch.AddRequestDispatchFilter<Rq, int>("rd", (ctx, next) => Task.FromResult(99)))
        {
            var r = await Dispatcher.RequestAsync(new Rq(), "rd");
            Assert.AreEqual(99, r);
        }

        var r2 = await Dispatcher.RequestAsync(new Rq(), "rd");
        Assert.AreEqual(7, r2);
    }

    [TestMethod]
    public async Task Disposable_StreamDispatchFilter_ShortCircuits_Then_Removed()
    {
        var dispatch = ServiceProvider.GetRequiredService<IDispatchFiltersRegister>();

        static async IAsyncEnumerable<int> Single()
        {
            yield return 42; await Task.CompletedTask;
        }

        await using (dispatch.AddStreamDispatchFilter<SReq, int>("sd", (ctx, next) => Single()))
        {
            var list = new List<int>();
            await foreach (var v in Dispatcher.StreamAsync(new SReq(), "sd")) list.Add(v);
            CollectionAssert.AreEqual(new[] { 42 }, list);
        }

        var list2 = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync(new SReq(), "sd")) list2.Add(v);
        CollectionAssert.AreEqual(new[] { 10, 11 }, list2);
    }
}

using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

public class StreamReq : IRequest<int> { }

[TestClass]
public class StreamFiltersAdvanced : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
    private int _handlerCount;

    private static async IAsyncEnumerable<int> BaseStream()
    {
        for (int i = 0; i < 3; i++) yield return i;
    }

    private static async IAsyncEnumerable<int> ShortCircuitStream(IStreamContext<StreamReq> ctx, StreamHandlingFilterNextDelegate<int> next)
    {
        yield return 100;
        yield return 200;
    }

    private static async IAsyncEnumerable<int> PlusOne(IStreamContext<StreamReq> ctx, StreamHandlingFilterNextDelegate<int> next)
    {
        await foreach (var v in next()) yield return v + 1;
    }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.AddStreamHandler<StreamReq, int>("stream", BaseStream)
               .WithFilter("stream", PlusOne);

        builder.AddStreamHandler<StreamReq, int>("short", BaseStream);
        builder.AddStreamFilter<StreamReq, int>("short", ShortCircuitStream);

        // Dispatch topic rewrite
        builder.AddStreamHandler<StreamReq, int>("final", () => { _handlerCount++; return BaseStream(); });
        builder.AddStreamDispatchFilter<StreamReq, int>("rewrite", (ctx, next) => next("final"));
    }

    [TestMethod]
    public async Task StreamHandling_ShortCircuit_YieldsOnlyCustomValues()
    {
        var list = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync(new StreamReq(), "short")) list.Add(v);
        CollectionAssert.AreEqual(new[] { 100, 200 }, list);
    }

    [TestMethod]
    public async Task StreamHandling_Filter_Order_Applies()
    {
        var list = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync(new StreamReq(), "stream")) list.Add(v);
        // Base: 0,1,2 => PlusOne: 1,2,3
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
    }

    [TestMethod]
    public async Task StreamDispatch_TopicRewrite_ChangesHandler()
    {
        var list = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync(new StreamReq(), "rewrite")) list.Add(v);
        Assert.AreEqual(3, list.Count);
        Assert.AreEqual(1, _handlerCount);
    }
}

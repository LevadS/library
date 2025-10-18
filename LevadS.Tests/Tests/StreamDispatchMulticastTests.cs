using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

[TestClass]
public class StreamDispatchMulticastTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private sealed class R : IRequest<int> { }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.AddStreamHandler<R, int>("s:a", () => Get(1));
        builder.AddStreamHandler<R, int>("s:b", () => Get(10));

        // Multicast dispatch filter: calls next twice to two different topics.
        builder.AddStreamDispatchFilter<R, int>("multi", (ctx, next) => FanOut(ctx, next));

        static async IAsyncEnumerable<int> Get(int baseVal)
        {
            yield return baseVal;
            yield return baseVal + 1;
            await Task.CompletedTask;
        }

        static async IAsyncEnumerable<int> FanOut(IStreamContext ctx, StreamDispatchFilterNextDelegate<int> next)
        {
            await foreach (var v in next("s:a")) yield return v;
            await foreach (var v in next("s:b")) yield return v;
        }
    }

    [TestMethod]
    public async Task StreamDispatch_Multicast_FanOutConcatsSequences()
    {
        var list = new List<int>();
        await foreach (var v in Dispatcher.StreamAsync(new R(), "multi")) list.Add(v);
        // Expect values from s:a (1,2) followed by s:b (10,11)
        CollectionAssert.AreEqual(new[] { 1, 2, 10, 11 }, list);
    }
}

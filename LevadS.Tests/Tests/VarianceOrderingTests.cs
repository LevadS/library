using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class VarianceOrderingTests : BaseTestClass
{
    private readonly List<string> _order = new();

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Derived first, then base, then object
        builder.AddMessageFilter<TestMessage>((ctx, next) => { _order.Add("derived"); return next(); });
        builder.AddMessageFilter<BaseMessage>((ctx, next) => { _order.Add("base"); return next(); });
        builder.AddMessageFilter<object>((ctx, next) => { _order.Add("object"); return next(); });

        // A simple handler to terminate the pipeline
        builder.AddMessageHandler<TestMessage>(() => Task.CompletedTask);
    }

    [TestMethod]
    public async Task MessageFilters_Variance_Order_DerivedThenBaseThenObject()
    {
        _order.Clear();
        await Dispatcher.SendAsync(new TestMessage());
        CollectionAssert.AreEqual(new[] { "derived", "base", "object" }, _order);
    }
}

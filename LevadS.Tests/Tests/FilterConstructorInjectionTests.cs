using LevadS.Interfaces;
using LevadS.Tests.Registrations.Filters;

namespace LevadS.Tests.Tests;

[TestClass]
public class FilterConstructorInjectionTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();
    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.RegisterServicesFromAssemblyContaining<CtorTopicRequestFilter>();
        builder.AddRequestHandler<CtorTopicRequestFilter.TestReq, int>("di:{n:int}", (IRequestContext<CtorTopicRequestFilter.TestReq> ctx) => (int)ctx.CapturedValues["n"]);
    }

    [TestMethod]
    public async Task FilterConstructor_FromTopic_InjectsIntoFilter()
    {
        CtorTopicRequestFilter.Seen = -1;
        var result = await Dispatcher.RequestAsync(new CtorTopicRequestFilter.TestReq(), "di:5");
        Assert.AreEqual(5, result);
        Assert.AreEqual(5, CtorTopicRequestFilter.Seen);
    }
}

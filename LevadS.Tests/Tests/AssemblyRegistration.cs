using LevadS.Interfaces;
using LevadS.Tests.Handlers;
using LevadS.Tests.Registrations.Filters;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class AssemblyRegistration : BaseTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();
    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.RegisterServicesFromAssemblyContaining<TestRequest>();
    }

    [TestMethod]
    public async Task PlainRequest()
    {
        BaseRequestFilter.Executed = false;
        ObjectRequestFilter.Executed = false;
        
        var response = await Dispatcher.RequestAsync(new TestRequest());
        
        Assert.AreEqual(nameof(TestResponse), response.GetType().Name);
        
        Assert.IsTrue(BaseRequestFilter.Executed);
        Assert.IsTrue(ObjectRequestFilter.Executed);
    }

    [TestMethod]
    public async Task PlainMessage()
    {
        BaseMessageFilter.Executed = false;
        ObjectMessageFilter.Executed = false;
        ObjectMessageDispatchFilter.Executed = false;
        
        await Dispatcher.SendAsync(new TestMessage());

        Assert.IsTrue(TestMessageHandler.Executed);
        Assert.IsTrue(BaseMessageFilter.Executed);
        Assert.IsTrue(ObjectMessageFilter.Executed);
        Assert.IsTrue(ObjectMessageDispatchFilter.Executed);
    }

    [TestMethod]
    public async Task GenericMessage()
    {
        GenericMessageHandler<int>.Executed = false;
        GenericMessageHandler<string>.Executed = false;
        GenericMessageHandler<float>.Executed = false;
        ObjectMessageFilter.Executed = false;
        ObjectMessageDispatchFilter.Executed = false;
                
        await Dispatcher.SendAsync(new GenericMessage<int>() { GenericPayload = 42 });
        await Dispatcher.SendAsync(new GenericMessage<string>() { GenericPayload = "The Answer" });
        
        Assert.IsTrue(GenericMessageHandler<int>.Executed);
        Assert.IsTrue(GenericMessageHandler<string>.Executed);
        Assert.IsFalse(GenericMessageHandler<float>.Executed);
        Assert.IsTrue(ObjectMessageFilter.Executed);
        Assert.IsTrue(ObjectMessageDispatchFilter.Executed);
    }

    [TestMethod]
    public async Task GenericRequest()
    {
        var intResponse = await Dispatcher.RequestAsync(new GenericRequest<int>());
        var stringResponse = await Dispatcher.RequestAsync(new GenericRequest<string>());
        
        Assert.AreEqual(0, intResponse);
        Assert.AreEqual(null, stringResponse);
    }
}
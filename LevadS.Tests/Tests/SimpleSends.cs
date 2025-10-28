using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class SimpleSends : BaseTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    private int _handlingCounter1 = 0;

    private int _handlingCounter2 = 0;

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.AddMessageHandler<SimpleMessage>("foo", () => _handlingCounter2++);
        
        builder.AddMessageHandler<SimpleMessage>("foo:#", () => _handlingCounter2++);
        
        builder.AddMessageHandler<SimpleMessage>("foo:*", () => _handlingCounter2++);
        
        builder.AddMessageHandler<SimpleMessage>("bar:+", () => _handlingCounter2++);
        
        builder.AddMessageHandler<SimpleMessage>("boo:{value:int}", (int value) =>
        {
            if (value == 42)
            {
                _handlingCounter2++;
            }
        });
        
        builder.AddMessageHandler<SimpleMessage>("boo:{value:string}", (string value) =>
        {
            if (value == "far")
            {
                _handlingCounter2++;
            }
        });
        
        builder.AddMessageHandler<OtherMessage>(() => _handlingCounter1++);
    }

    [TestMethod]
    public async Task Simple()
    {
        await Dispatcher.SendAsync(new OtherMessage());
        
        Assert.AreEqual(1, _handlingCounter1);
    }

    [TestMethod]
    public async Task SimpleTopic()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "foo");
        
        Assert.AreEqual(1, _handlingCounter2);
    }

    [TestMethod]
    public async Task WildcardTopic()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "foo:bar");
        
        Assert.AreEqual(1, _handlingCounter2);
    }

    [TestMethod]
    public async Task MultipleWildcardTopic()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "foo:bar:baz");
        
        Assert.AreEqual(1, _handlingCounter2);
    }

    [TestMethod]
    public async Task MultiplePlusWildcardTopic()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "bar:baz:foo");
        
        Assert.AreEqual(1, _handlingCounter2);
    }

    [TestMethod]
    public async Task MultiplePlusWildcardTopic_Negative()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await Dispatcher.SendAsync(new SimpleMessage(), "bar");
        });

        Assert.AreEqual(0, _handlingCounter2);
    }

    [TestMethod]
    public async Task StringCapture()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "boo:far");
        
        Assert.AreEqual(1, _handlingCounter2);
    }

    [TestMethod]
    public async Task IntCapture()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "boo:42");
        
        Assert.AreEqual(1, _handlingCounter2);
    }
}
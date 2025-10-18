using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class SimplePublishes : BaseTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    private int _handlingCounter = 0;

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.AddMessageHandler<SimpleMessage>(() => _handlingCounter++);
        
        builder.AddMessageHandler<SimpleMessage>("foo", () => _handlingCounter++);
        
        builder.AddMessageHandler<SimpleMessage>("foo:#", () => _handlingCounter++);
        
        builder.AddMessageHandler<SimpleMessage>("foo:*", () => _handlingCounter++);
        
        builder.AddMessageHandler<SimpleMessage>("boo:{value}", (string value) =>
        {
            if (value == "far")
            {
                _handlingCounter++;
            }
        });
        
        builder.AddMessageHandler<SimpleMessage>("boo:{value:int}", (int value) =>
        {
            if (value == 42)
            {
                _handlingCounter++;
            }
        });
    }

    [TestMethod]
    public async Task Simple()
    {
        await Dispatcher.PublishAsync(new SimpleMessage());
        
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task SimpleTopic()
    {
        await Dispatcher.PublishAsync(new SimpleMessage(), "foo");
        
        Assert.AreEqual(3, _handlingCounter);
    }

    [TestMethod]
    public async Task WildcardTopic()
    {
        await Dispatcher.PublishAsync(new SimpleMessage(), "foo:bar");
        
        Assert.AreEqual(3, _handlingCounter);
    }

    [TestMethod]
    public async Task MultipleWildcardTopic()
    {
        await Dispatcher.PublishAsync(new SimpleMessage(), "foo:bar:baz");
        
        Assert.AreEqual(2, _handlingCounter);
    }

    [TestMethod]
    public async Task StringCapture()
    {
        await Dispatcher.PublishAsync(new SimpleMessage(), "boo:far");
        
        Assert.AreEqual(2, _handlingCounter);
    }

    [TestMethod]
    public async Task IntCapture()
    {
        await Dispatcher.PublishAsync(new SimpleMessage(), "boo:42");
        
        Assert.AreEqual(2, _handlingCounter);
    }
}
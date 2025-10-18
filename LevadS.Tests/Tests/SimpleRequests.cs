using LevadS.Classes;
using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class SimpleRequests : BaseTestClass
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
        builder.AddRequestHandler<SimpleRequest, SimpleResponse>(() =>
        {
            _handlingCounter++;
            return new SimpleResponse();
        });
        
        builder.AddRequestHandler<Request<string, SimpleResponse>, SimpleResponse>(() =>
        {
            _handlingCounter++;
            return new SimpleResponse();
        });
        
        builder.AddRequestHandler<SimpleRequest, SimpleResponse>("foo", () =>
        {
            _handlingCounter++;
            return new SimpleResponse();
        });
        
        builder.AddRequestHandler<SimpleRequest, SimpleResponse>("foo:#", () =>
        {
            _handlingCounter++;
            return new SimpleResponse();
        });
        
        builder.AddRequestHandler<SimpleRequest, SimpleResponse>("foo:*", () =>
        {
            _handlingCounter++;
            return new SimpleResponse();
        });
        
        builder.AddRequestHandler<SimpleRequest, SimpleResponse>("boo:{value}", (string value) =>
        {
            if (value == "far")
            {
                _handlingCounter++;
            }
            return new SimpleResponse();
        });
        
        builder.AddRequestHandler<SimpleRequest, SimpleResponse>("boo:{value:int}", (int value) =>
        {
            if (value == 42)
            {
                _handlingCounter++;
            }
            return new SimpleResponse();
        });
    }

    [TestMethod]
    public async Task Simple()
    {
        var response = await Dispatcher.RequestAsync(new SimpleRequest());
        
        Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task SimpleTopic()
    {
        var response = await Dispatcher.RequestAsync(new SimpleRequest(), "foo");
        
        Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task WildcardTopic()
    {
        var response = await Dispatcher.RequestAsync(new SimpleRequest(), "foo:bar");
        
        Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task MultipleWildcardTopic()
    {
        var response = await Dispatcher.RequestAsync(new SimpleRequest(), "foo:bar:baz");
        
        Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task StringCapture()
    {
        var response = await Dispatcher.RequestAsync(new SimpleRequest(), "boo:far");
        
        Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task IntCapture()
    {
        var response = await Dispatcher.RequestAsync(new SimpleRequest(), "boo:42");
        
        Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
        Assert.AreEqual(1, _handlingCounter);
    }
}
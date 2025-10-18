using LevadS.Classes;
using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class SimpleStreams : BaseTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    private int _handlingCounter = 0;

    private async IAsyncEnumerable<SimpleResponse> HandleStream()
    {
        _handlingCounter++;
        yield return new SimpleResponse();
    }

    private async IAsyncEnumerable<SimpleResponse> HandleStreamWithInt(int value)
    {
        if (value == 42)
        {
            _handlingCounter++;
        }
        yield return new SimpleResponse();
    }

    protected async IAsyncEnumerable<SimpleResponse> HandleStreamWithString(string value)
    {
        if (value == "far")
        {
            _handlingCounter++;
        }
        yield return new SimpleResponse();
    }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.AddStreamHandler<SimpleRequest, SimpleResponse>(HandleStream);
        
        builder.AddStreamHandler<Request<string, SimpleResponse>, SimpleResponse>(HandleStream);
        
        builder.AddStreamHandler<SimpleRequest, SimpleResponse>("foo", HandleStream);
        
        builder.AddStreamHandler<SimpleRequest, SimpleResponse>("foo:#", HandleStream);
        
        builder.AddStreamHandler<SimpleRequest, SimpleResponse>("foo:*", HandleStream);
        
        builder.AddStreamHandler<SimpleRequest, SimpleResponse>("boo:{value}", HandleStreamWithInt);
        
        builder.AddStreamHandler<SimpleRequest, SimpleResponse>("boo:{value:int}", HandleStreamWithString);
    }

    [TestMethod]
    public async Task Simple()
    {
        var stream = Dispatcher.StreamAsync(new SimpleRequest());
        await foreach (var response in stream)
        {
            Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
            Assert.AreEqual(1, _handlingCounter);
            
            return;
        }
    }

    [TestMethod]
    public async Task SimpleTopic()
    {
        var stream = Dispatcher.StreamAsync(new SimpleRequest(), "foo");
        await foreach (var response in stream)
        {
            Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
            Assert.AreEqual(1, _handlingCounter);
            
            return;
        }
    }

    [TestMethod]
    public async Task WildcardTopic()
    {
        var stream = Dispatcher.StreamAsync(new SimpleRequest(), "foo:bar");
        await foreach (var response in stream)
        {
            Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
            Assert.AreEqual(1, _handlingCounter);
            
            return;
        }
    }

    [TestMethod]
    public async Task MultipleWildcardTopic()
    {
        var stream = Dispatcher.StreamAsync(new SimpleRequest(), "foo:bar:baz");
        await foreach (var response in stream)
        {
            Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
            Assert.AreEqual(1, _handlingCounter);
            
            return;
        }
    }

    [TestMethod]
    public async Task StringCapture()
    {
        var stream = Dispatcher.StreamAsync(new SimpleRequest(), "boo:far");
        await foreach (var response in stream)
        {
            Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
            Assert.AreEqual(1, _handlingCounter);
            
            return;
        }
    }

    [TestMethod]
    public async Task IntCapture()
    {
        var stream = Dispatcher.StreamAsync(new SimpleRequest(), "boo:42");
        await foreach (var response in stream)
        {
            Assert.AreEqual(nameof(SimpleResponse), response.GetType().Name);
            Assert.AreEqual(1, _handlingCounter);
            
            return;
        }
    }
}
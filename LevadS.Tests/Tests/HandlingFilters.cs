using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Tests.Handlers;
using LevadS.Tests.Registrations.Filters;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Tests;

public class IntRequest : IRequest<int>
{
    public int RequestedId { get; set; }
}

[TestClass]
public class HandlingFilters : BaseTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    private int _handlingCounter = 0;
    
    private Dictionary<int, int> _responseCache = new Dictionary<int, int>();

    private static async IAsyncEnumerable<int> HandleStream()
    {
        foreach (var i in Enumerable.Range(0, 100))
        {
            yield return i;
        }
    }

    private static async IAsyncEnumerable<int> FilterStream(IStreamContext<IntRequest> requestContext, StreamHandlingFilterNextDelegate<int> next)
    {
        await foreach (var i in next())
        {
            yield return i + 100;
        }
    }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.AddRequestHandler<IntRequest, int>("baz", (IRequestContext<IntRequest> ctx) => (int)ctx.Headers.GetValueOrDefault("boo", 0));
        
        builder.AddRequestFilter<IntRequest, int>("baz", (context, next) => next(new Dictionary<string, object>(context.Headers) { { "boo", 40 } }));
        
        builder.AddRequestFilter<IntRequest, int>("baz", (context, next) =>
        {
            var headers = new Dictionary<string, object>(context.Headers);
            if (headers.TryGetValue("boo", out var boo))
            {
                headers["boo"] = (int)boo + 2;
            }
            
            return next(headers);
        });
        
        builder.AddRequestHandler<IntRequest, int>("foo", () => 41);
        
        builder.AddRequestFilter<IntRequest, int>("foo", async (ctx, next) => await next() + 1);

        builder.AddRequestHandler<IntRequest, int>("bar:#:{v:int}", (int v) => v);
        
        builder.AddRequestFilter<IntRequest, int>("bar:{v:int}:#", async (ctx, next) => await next() + (int)ctx.CapturedValues.GetValueOrDefault("v", 0));
        
        builder.AddRequestFilter<IntRequest, int>("bar:{x:int}:#", async (ctx, next) => await next() + (int)ctx.CapturedValues.GetValueOrDefault("x", 0));

        builder.AddMessageHandler<IntRequest>("bar:#:{v:int}", (int v) => _handlingCounter += v);
        
        builder.AddMessageFilter<IntRequest>("bar:{v:int}:#", (ctx, next) => {
            _handlingCounter += (int)ctx.CapturedValues.GetValueOrDefault("v", 0);
            return next();
        });
        
        builder.AddMessageFilter<IntRequest>("bar:{x:int}:#", (ctx, next) => {
            _handlingCounter += (int)ctx.CapturedValues.GetValueOrDefault("x", 0);
            return next();
        });
        
        builder.AddRequestHandler<IntRequest, int>("boo", () => _handlingCounter++);
        
        builder.AddRequestFilter<IntRequest, int>("boo", async (ctx, next) =>
        {
            var result = 0;
            do
            {
                result = await next();
            } while (result < 5);
            
            return result;
        });
        
        builder.AddRequestHandler<IntRequest, int>("far", (IntRequest req) =>
        {
            _handlingCounter++;

            return req.RequestedId;
        });
        
        builder.AddRequestFilter<IntRequest, int>("far", async (ctx, next) =>
        {
            if (!_responseCache.TryGetValue(ctx.Request.RequestedId, out var result))
            {
                result = await next();
                
                _responseCache[ctx.Request.RequestedId] = result;
            }
            
            return result;
        });
        
        builder.AddStreamHandler<IntRequest, int>(HandleStream);
        
        builder.AddStreamFilter<IntRequest, int>(FilterStream);

        builder.AddMessageHandler<GenericMessage<string>, GenericMessageHandler<string>>();
        builder.AddMessageHandler<GenericMessage<int>, GenericMessageHandler<int>>();
        builder.AddRequestHandler<GenericRequest<string>, string, GenericRequestHandler<string>>();
        builder.AddRequestHandler<GenericRequest<int>, int, GenericRequestHandler<int>>();
        builder.AddMessageFilter<object, ObjectMessageFilter>();
        builder.AddMessageDispatchFilter<object, ObjectMessageDispatchFilter>();
    }

    [TestMethod]
    public async Task ResultChange()
    {
        var result = await Dispatcher.RequestAsync(new IntRequest(), "foo");
        
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task HeadersChange()
    {
        var result = await Dispatcher.RequestAsync(new IntRequest(), "baz");
        
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task ResultChangeWithCapture()
    {
        var result = await Dispatcher.RequestAsync(new IntRequest(), "bar:1:40");
        
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public async Task MessageFiltersWithCapture()
    {
        await Dispatcher.SendAsync(new IntRequest(), "bar:1:40");
        
        Assert.AreEqual(42, _handlingCounter);
    }

    [TestMethod]
    public async Task RequestRetry()
    {
        var result = await Dispatcher.RequestAsync(new IntRequest(), "boo");
        
        Assert.IsTrue(result >= 5);
    }

    [TestMethod]
    public async Task ResponseCache()
    {
        var results = await Task.WhenAll(
        Enumerable.Range(0, 100).Select(v => Dispatcher.RequestAsync(new IntRequest() { RequestedId = v / 10 }, "far"))
        );
        
        Assert.IsTrue(_handlingCounter <= 10);
    }

    [TestMethod]
    public async Task StreamChanging()
    {
        var hit = false;
        var stream = Dispatcher.StreamAsync(new IntRequest());
        await foreach (var response in stream)
        {
            Assert.IsTrue(response >= 100);

            hit = true;
        }
        
        Assert.IsTrue(hit);
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
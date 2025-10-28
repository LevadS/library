using LevadS.Interfaces;
using LevadS.Tests.Messages;

namespace LevadS.Tests.Tests;

[TestClass]
public class DispatchFilters : BaseTestClass
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
        builder.AddMessageHandler<SimpleMessage>("foo:bar", () => _handlingCounter++);

        builder.AddMessageDispatchFilter<SimpleMessage>("foo", (context, next) => next($"{context.Topic}:bar"));
        
        builder.AddMessageHandler<SimpleMessage>("boo:far", (IMessageContext<SimpleMessage> ctx) =>
        {
            if (ctx.Headers.ContainsKey("boo") && ctx.Headers.ContainsKey("increment"))
            {
                _handlingCounter++;
            }
        });
        
        builder.AddMessageDispatchFilter<SimpleMessage>("boo:far", (context, next) => next(headers: new Dictionary<string, object>(context.Headers) { { "increment", "1" } }));
        
        builder.AddMessageHandler<SimpleMessage>("zoo:baz", () => _handlingCounter++);
        
        builder.AddMessageHandler<SimpleMessage>("zoo:faz", () => _handlingCounter++);

        builder.AddMessageDispatchFilter<SimpleMessage>("zoo", (context, next) => Task.WhenAll(
            next($"{context.Topic}:baz"),
            next($"{context.Topic}:faz")
        ));
    }

    [TestMethod]
    public async Task TopicRewrite()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "foo");
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await Dispatcher.SendAsync(new SimpleMessage(), "boo");
        });

        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task HeadersRewrite()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "boo:far", headers: new Dictionary<string, object> { { "boo", "1" } });
        
        Assert.AreEqual(1, _handlingCounter);
    }

    [TestMethod]
    public async Task TopicMulticast()
    {
        await Dispatcher.SendAsync(new SimpleMessage(), "zoo");
        
        Assert.AreEqual(2, _handlingCounter);
    }
}
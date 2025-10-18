using System;
using LevadS;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class NegativeRegistrationTests
{
    private sealed class DummyReq : IRequest<int> { }

    [TestMethod]
    public void AddMessageHandler_InvalidPattern_Throws()
    {
        var sc = new ServiceCollection();
        Assert.ThrowsException<ArgumentException>(() =>
        {
            sc.AddLevadS(b =>
            {
                // Empty pattern is invalid
                b.AddMessageHandler<DummyMessage>("", () => { });
            });
        });
    }

    private sealed class DummyMessage { }

    [TestMethod]
    public void AddRequestHandler_InvalidPattern_Throws()
    {
        var sc = new ServiceCollection();
        Assert.ThrowsException<ArgumentException>(() =>
        {
            sc.AddLevadS(b =>
            {
                // Double separator creates an empty part
                b.AddRequestHandler<DummyReq, int>("foo::bar", () => 1);
            });
        });
    }

    [TestMethod]
    public void AddStreamHandler_InvalidPattern_Throws()
    {
        var sc = new ServiceCollection();
        Assert.ThrowsException<ArgumentException>(() =>
        {
            sc.AddLevadS(b =>
            {
                // Unknown capture type should be rejected by validation
                b.AddStreamHandler<DummyReq, int>("bad:{x:unknown}", () => Get());
            });
        });

        static async IAsyncEnumerable<int> Get()
        {
            yield return 1;
            await Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task Request_NoHandler_Throws()
    {
        var sc = new ServiceCollection();
        sc.AddLevadS(_ => { /* no handlers */ });
        var sp = sc.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IDispatcher>();

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await dispatcher.RequestAsync(new DummyReq(), "missing");
        });

        StringAssert.Contains(ex.Message, nameof(DummyReq));
    }
}

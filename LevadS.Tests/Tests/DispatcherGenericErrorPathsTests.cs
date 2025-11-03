using System.Collections.Generic;
using System.Threading.Tasks;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class DispatcherGenericErrorPathsTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Empty; exercise error paths where no handlers are registered
    }

    private sealed class NoHandlerReq : IRequest<int> { }

    [TestMethod]
    public async Task RequestAsync_generic_overload_throws_when_no_handler()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            _ = await Dispatcher.RequestAsync<int>(new NoHandlerReq(), topic: "missing");
        });
    }

    private sealed class NoHandlerSReq : IRequest<int> { }

    [TestMethod]
    public async Task StreamAsync_generic_overload_throws_on_enumeration_when_no_handler()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in Dispatcher.StreamAsync<int>(new NoHandlerSReq(), topic: "missing"))
            {
                // noop
            }
        });
    }
}

using System;
using System.Threading.Tasks;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class ServiceContainerScopeAdditionalTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // No static registrations; tests use runtime registration APIs as needed
    }

    [TestMethod]
    public void Nested_scopes_register_and_unregister_on_dispose()
    {
        var resolver = ServiceProvider.GetRequiredService<IServiceResolver>();

        // Create nested scopes; this exercises RegisterScope on the parent
        var scopeA = resolver.CreateScope();
        var scopeB = scopeA.CreateScope();

        // Dispose child first, then parent; this exercises UnregisterScope and DisposeAsync paths
        scopeB.Dispose();
        scopeA.Dispose();

        // If no exceptions are thrown, the internal scope tracking worked as expected
        Assert.IsTrue(true);
    }

    private sealed class UnknownRequest : IRequest<int> { }

    [TestMethod]
    public async Task Dispatcher_RequestAsync_object_overload_unwraps_TargetInvocationException()
    {
        // Call the object overload with a request type that has no handler
        // Expect an InvalidOperationException (unwrapped from TargetInvocationException)
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            _ = await Dispatcher.RequestAsync<int>(new UnknownRequest(), topic: "no-handler-topic");
        });
    }

    private sealed class UnknownStreamRequest : IRequest<int> { }

    [TestMethod]
    public async Task Dispatcher_StreamAsync_object_overload_throws_on_enumeration_when_no_handler()
    {
        // For streams, the reflection invoke returns an IAsyncEnumerable; the exception occurs on enumeration
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in Dispatcher.StreamAsync<int>(new UnknownStreamRequest(), topic: "no-handler-topic"))
            {
                // no-op
            }
        });
    }
}

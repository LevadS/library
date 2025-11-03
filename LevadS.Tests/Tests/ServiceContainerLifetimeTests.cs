using System;
using System.Threading.Tasks;
using LevadS;
using LevadS.Classes.Envelopers;
using LevadS.Interfaces;
using LevadS.Tests.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevadS.Tests.Tests;

[TestClass]
public class ServiceContainerLifetimeTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // No default registrations; tests will register via IServiceRegister directly
    }

    private static void ResetStatics()
    {
        ScopedHandler.InstanceCount = 0;
        TrackableHandler.CreatedInstance = null;
    }

    [TestMethod]
    public async Task Scoped_handler_requires_explicit_scope()
    {
        ResetStatics();
        var register = ServiceProvider.GetRequiredService<IServiceRegister>();
        using var reg = register.Register<IMessageHandler<SimpleMessage>, ScopedHandler, SimpleMessage>(
            new NoopEnveloper(),
            topicPattern: "orders:*",
            factory: _ => new ScopedHandler(),
            lifetime: ServiceLifetime.Scoped
        );

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await Dispatcher.SendAsync(new SimpleMessage(), topic: "orders:created");
        });
    }

    [TestMethod]
    public async Task Singleton_handler_disposed_when_registration_handle_disposed()
    {
        ResetStatics();
        var register = ServiceProvider.GetRequiredService<IServiceRegister>();
        var reg = register.Register<IMessageHandler<SimpleMessage>, TrackableHandler, SimpleMessage>(
            new NoopEnveloper(),
            topicPattern: "orders:*",
            factory: _ => new TrackableHandler(),
            lifetime: ServiceLifetime.Singleton
        );

        // Resolve once to create the singleton
        await Dispatcher.SendAsync(new SimpleMessage(), topic: "orders:1");
        Assert.IsNotNull(TrackableHandler.CreatedInstance);
        Assert.IsFalse(TrackableHandler.CreatedInstance!.IsDisposed);

        // Disposing registration should dispose singleton instance
        reg.Dispose();
        Assert.IsTrue(TrackableHandler.CreatedInstance!.IsDisposed);
    }

    [TestMethod]
    public async Task Singleton_handler_disposed_on_container_dispose()
    {
        ResetStatics();
        var register = ServiceProvider.GetRequiredService<IServiceRegister>();
        using var reg = register.Register<IMessageHandler<SimpleMessage>, TrackableHandler, SimpleMessage>(
            new NoopEnveloper(),
            topicPattern: "orders:*",
            factory: _ => new TrackableHandler(),
            lifetime: ServiceLifetime.Singleton
        );

        // Resolve once to create the singleton and track it
        await Dispatcher.SendAsync(new SimpleMessage(), topic: "orders:2");
        Assert.IsNotNull(TrackableHandler.CreatedInstance);
        Assert.IsFalse(TrackableHandler.CreatedInstance!.IsDisposed);

        // Dispose the resolver/container to trigger singleton disposal
        var resolver = ServiceProvider.GetRequiredService<IServiceResolver>();
        resolver.Dispose();

        Assert.IsTrue(TrackableHandler.CreatedInstance!.IsDisposed);
    }

    private sealed class ScopedHandler : IMessageHandler<SimpleMessage>
    {
        public static int InstanceCount;
        public ScopedHandler()
        {
            InstanceCount++;
        }
        public Task HandleAsync(IMessageContext<SimpleMessage> ctx) => Task.CompletedTask;
    }

    private sealed class TrackableHandler : IMessageHandler<SimpleMessage>, IDisposable
    {
        public static TrackableHandler? CreatedInstance;
        public bool IsDisposed { get; private set; }
        public TrackableHandler()
        {
            CreatedInstance = this;
        }
        public Task HandleAsync(IMessageContext<SimpleMessage> ctx) => Task.CompletedTask;
        public void Dispose() => IsDisposed = true;
    }
}

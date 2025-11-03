using System;
using System.Threading.Tasks;
using LevadS;
using LevadS.Classes.Envelopers;
using LevadS.Interfaces;
using LevadS.Tests.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class ServiceContainerAsyncSingletonTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // No default registrations in this suite
    }

    private sealed class AsyncTrackableHandler : IMessageHandler<SimpleMessage>, IAsyncDisposable
    {
        public static AsyncTrackableHandler? Instance;
        public bool Disposed { get; private set; }

        public AsyncTrackableHandler() => Instance = this;

        public Task HandleAsync(IMessageContext<SimpleMessage> ctx) => Task.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            // Simulate async cleanup
            await Task.Yield();
            Disposed = true;
        }
    }

    [TestMethod]
    public async Task Async_singleton_is_disposed_when_registration_handle_disposed()
    {
        AsyncTrackableHandler.Instance = null;

        var register = ServiceProvider.GetRequiredService<IServiceRegister>();
        var handle = register.Register<IMessageHandler<SimpleMessage>, AsyncTrackableHandler, SimpleMessage>(
            new NoopEnveloper(),
            topicPattern: "async:singleton",
            factory: _ => new AsyncTrackableHandler(),
            lifetime: ServiceLifetime.Singleton
        );

        // Resolve once to create the singleton
        await Dispatcher.SendAsync(new SimpleMessage(), topic: "async:singleton");
        Assert.IsNotNull(AsyncTrackableHandler.Instance, "Handler instance should be created");
        Assert.IsFalse(AsyncTrackableHandler.Instance!.Disposed, "Handler should not be disposed yet");

        // Disposing the registration handle must dispose the async singleton
        handle.Dispose();
        Assert.IsTrue(AsyncTrackableHandler.Instance!.Disposed, "Async singleton should be disposed on handle.Dispose()");
    }

    [TestMethod]
    public async Task Async_singleton_is_disposed_on_container_DisposeAsync()
    {
        AsyncTrackableHandler.Instance = null;

        var register = ServiceProvider.GetRequiredService<IServiceRegister>();
        using var handle = register.Register<IMessageHandler<SimpleMessage>, AsyncTrackableHandler, SimpleMessage>(
            new NoopEnveloper(),
            topicPattern: "async:container",
            factory: _ => new AsyncTrackableHandler(),
            lifetime: ServiceLifetime.Singleton
        );

        await Dispatcher.SendAsync(new SimpleMessage(), topic: "async:container");
        Assert.IsNotNull(AsyncTrackableHandler.Instance);
        Assert.IsFalse(AsyncTrackableHandler.Instance!.Disposed);

        var resolver = ServiceProvider.GetRequiredService<IServiceResolver>();
        await resolver.DisposeAsync();

        Assert.IsTrue(AsyncTrackableHandler.Instance!.Disposed, "Async singleton should be disposed when container is disposed");
    }
}

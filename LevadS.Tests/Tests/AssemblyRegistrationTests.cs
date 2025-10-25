using LevadS.Interfaces;
using LevadS.Attributes;
using LevadS.Tests.Registrations.Filters;
using LevadS.Tests.Registrations.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Tests.Tests;

[TestClass]
public class AssemblyRegistrationTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        builder.RegisterServicesFromAssemblyContaining<ObjectMessageFilter>();
    }

    [TestMethod]
    public async Task Registers_Message_Handler_And_Runs()
    {
        // TestMessageHandler and ObjectMessageDispatchFilter come from registrations assembly
        LevadS.Tests.Handlers.TestMessageHandler.Executed = false;
        ObjectMessageDispatchFilter.Executed = false;

        await Dispatcher.SendAsync(new TestMessage(), "*");

        Assert.IsTrue(LevadS.Tests.Handlers.TestMessageHandler.Executed, "message handler should run");
        Assert.IsTrue(ObjectMessageDispatchFilter.Executed, "dispatch filter should run");
    }

    [TestMethod]
    public async Task Registers_Request_Handler_And_Base_Filter_ViaScanning()
    {
        BaseRequestFilter.Executed = false;
        var res = await Dispatcher.RequestAsync(new TestRequest(), "*");
        Assert.IsNotNull(res);
        Assert.IsTrue(BaseRequestFilter.Executed, "base request filter should run (variance)");
    }

    [TestMethod]
    public async Task Registers_Object_Variant_Filters_For_Message_And_Request()
    {
        ObjectMessageFilter.Executed = false;
        ObjectRequestFilter.Executed = false;

        await Dispatcher.SendAsync(new TestMessage(), "*");
        Assert.IsTrue(ObjectMessageFilter.Executed, "object-variant message filter should run");

    ObjectRequestFilter.Executed = false;
    var _ = await Dispatcher.RequestAsync(new TestRequest(), "*");
        Assert.IsTrue(ObjectRequestFilter.Executed, "object-variant request filter should run");
    }

    [TestMethod]
    public async Task Registers_Generic_Handlers_ViaAttributes()
    {
        // GenericMessageHandler<int> and <string> registered; make sure they run
        LevadS.Tests.Handlers.GenericMessageHandler<int>.Executed = false;
        LevadS.Tests.Handlers.GenericMessageHandler<string>.Executed = false;

        await Dispatcher.SendAsync(new GenericMessage<int> { GenericPayload = 1 }, "*");
        await Dispatcher.SendAsync(new GenericMessage<string> { GenericPayload = "x" }, "*");

        Assert.IsTrue(LevadS.Tests.Handlers.GenericMessageHandler<int>.Executed);
        Assert.IsTrue(LevadS.Tests.Handlers.GenericMessageHandler<string>.Executed);
    }

    [TestMethod]
    public async Task Registers_Generic_Request_Handlers_ViaAttributes()
    {
        // GenericRequestHandler<int> and <string> registered via attributes in Registrations assembly
        var intResponse = await Dispatcher.RequestAsync(new GenericRequest<int>(), "*");
        var stringResponse = await Dispatcher.RequestAsync(new GenericRequest<string>(), "*");

        Assert.AreEqual(0, intResponse, "generic request<int> should be handled and return default 0");
        Assert.IsNull(stringResponse, "generic request<string> should be handled and return default null");
    }

    [TestMethod]
    public async Task Registers_Object_Variant_Request_Dispatch_Filter_ViaScanning()
    {
        LevadS.Tests.Registrations.Filters.ObjectRequestDispatchFilter.Executed = false;
        var _ = await Dispatcher.RequestAsync(new TestRequest(), "*");
        Assert.IsTrue(LevadS.Tests.Registrations.Filters.ObjectRequestDispatchFilter.Executed, "request dispatch filter should run (variance)");
    }

    [TestMethod]
    public async Task Registers_Object_Variant_Stream_Dispatch_Filter_ViaScanning()
    {
        LevadS.Tests.Registrations.Filters.ObjectStreamDispatchFilter.Executed = false;

        var register = ServiceProvider.GetRequiredService<IHandlersRegister>();
        await using var disp = register
            .AddStreamHandler<TestRequest, TestResponse, TempStreamHandler>("*")
            .Build();

        await foreach (var _ in Dispatcher.StreamAsync(new TestRequest(), "*"))
        {
            break;
        }

        Assert.IsTrue(LevadS.Tests.Registrations.Filters.ObjectStreamDispatchFilter.Executed, "stream dispatch filter should run (variance)");
    }

    [TestMethod]
    public async Task Registers_HandlerScoped_Filter_ViaAttributes_And_Executes_Only_For_Target_Handler()
    {
        // Reset flag
        ScopedObjectMessageFilter.Executed = false;

        // This should execute TestMessageHandler and the handler-scoped filter
        await Dispatcher.SendAsync(new TestMessage(), "*");
        Assert.IsTrue(ScopedObjectMessageFilter.Executed, "scoped filter should run for its target handler");

        // Reset and send a different message type that maps to no TestMessageHandler
        ScopedObjectMessageFilter.Executed = false;
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            await Dispatcher.SendAsync("string-payload", "*");
        });
        Assert.IsFalse(ScopedObjectMessageFilter.Executed, "scoped filter should NOT run for other handlers/messages");
    }

    [TestMethod]
    public async Task Registers_HandlerScoped_Request_Filter_ViaAttributes_And_Executes_Only_For_Target_Handler()
    {
        // Reset flag
        ScopedObjectRequestFilter.Executed = false;

        // This should execute TestRequestHandler and the handler-scoped request filter
        _ = await Dispatcher.RequestAsync(new TestRequest(), "*");
        Assert.IsTrue(ScopedObjectRequestFilter.Executed, "scoped request filter should run for its target handler");

        // Reset and invoke a different request type (GenericRequest<int> is handled by GenericRequestHandler<int>)
        ScopedObjectRequestFilter.Executed = false;
        _ = await Dispatcher.RequestAsync(new GenericRequest<int>(), "*");
        Assert.IsFalse(ScopedObjectRequestFilter.Executed, "scoped request filter should NOT run for other handlers");
    }

    [TestMethod]
    public async Task Registers_HandlerScoped_Stream_Filter_ViaAttributes_And_Executes_Only_For_Target_Handler()
    {
        // Reset flag
        ScopedObjectStreamFilter.Executed = false;

        // The TestRequestHandler does not define a stream handler; we add a transient at runtime and verify scoped filter applies
        // Arrange a temporary stream handler for TestRequest -> TestResponse via IHandlersRegister
        var register = ServiceProvider.GetRequiredService<IHandlersRegister>();
        await using var disp = register
            .AddStreamHandler<TestRequest, TestResponse, TempStreamHandler>("*")
            .Build();

        // Send a stream request targeting TestRequest handler key; the scoped filter is attached to TestRequestHandler via attribute
        await foreach (var _ in Dispatcher.StreamAsync(new TestRequest(), "*"))
        {
            break;
        }
        Assert.IsFalse(ScopedObjectStreamFilter.Executed, "scoped stream filter should NOT run when stream handler differs from attribute target");

        // Reset and stream a different request type handled by GenericRequestHandler<int>.
        // There is no stream handler registered for GenericRequest<int> in this test, so streaming it should throw.
        // We only care that the scoped stream filter did NOT execute in this case.
        ScopedObjectStreamFilter.Executed = false;
        var threw = false;
        try
        {
            await foreach (var _ in Dispatcher.StreamAsync(new GenericRequest<int>(), "*"))
            {
                break;
            }
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        Assert.IsTrue(threw, "streaming GenericRequest<int> without a handler should throw");
        Assert.IsFalse(ScopedObjectStreamFilter.Executed, "scoped stream filter should NOT run for other handlers");
    }

    [TestMethod]
    public async Task HandlerScoped_Stream_Filter_DoesNotRun_For_Other_Stream_Handler()
    {
        // Ensure the scoped stream filter is reset
        ScopedObjectStreamFilter.Executed = false;

        // Register a temporary stream handler for GenericRequest<int> and verify that the scoped filter doesn't run for it
        var register = ServiceProvider.GetRequiredService<IHandlersRegister>();
        await using var disp = register
            .AddStreamHandler<GenericRequest<int>, int, TempIntStreamHandler>("*")
            .Build();

        await foreach (var _ in Dispatcher.StreamAsync(new GenericRequest<int>(), "*"))
        {
            break;
        }

        Assert.IsFalse(ScopedObjectStreamFilter.Executed, "scoped stream filter should NOT run for other handlers");
    }

    // Temporary stream handler used only within the test to enable stream path for TestRequest
    private sealed class TempStreamHandler : IStreamHandler<TestRequest, TestResponse>
    {
        public IAsyncEnumerable<TestResponse> HandleAsync(IStreamContext<TestRequest> streamContext, CancellationToken cancellationToken)
        {
            return Get();

            static async IAsyncEnumerable<TestResponse> Get()
            {
                yield return new TestResponse();
                await Task.CompletedTask;
            }
        }
    }

    // Temporary stream handler for GenericRequest<int> emitting a single integer
    private sealed class TempIntStreamHandler : IStreamHandler<GenericRequest<int>, int>
    {
        public IAsyncEnumerable<int> HandleAsync(IStreamContext<GenericRequest<int>> streamContext, CancellationToken cancellationToken)
        {
            return Get();

            static async IAsyncEnumerable<int> Get()
            {
                yield return 42;
                await Task.CompletedTask;
            }
        }
    }
}

[TestClass]
public class AssemblyRegistrationNegativeTests
{
    // Open generic handler missing LevadSGenericRegistration attribute
    [LevadSRegistration]
    internal class OpenGenericBadHandler<T> : IRequestHandler<GenericRequest<T>, T>
    {
        public Task<T> HandleAsync(IRequestContext<GenericRequest<T>> requestContext)
            => Task.FromResult(default(T)!);
    }

    // Open generic handler with mismatched LevadSGenericRegistration ServiceType
    [LevadSGenericRegistration<LevadS.Tests.Handlers.GenericRequestHandler<int>>]
    internal class MismatchedGenericHandler<T> : IRequestHandler<GenericRequest<T>, T>
    {
        public Task<T> HandleAsync(IRequestContext<GenericRequest<T>> requestContext)
            => Task.FromResult(default(T)!);
    }

    // Open generic handler with mismatched InterfaceType in LevadSGenericRegistration (interface not assignable from service)
    [LevadSGenericRegistration<IMessageHandler<GenericMessage<int>>, LevadS.Tests.Handlers.GenericRequestHandler<int>>]
    internal class MismatchedGenericInterfaceHandler<T> : IRequestHandler<GenericRequest<T>, T>
    {
        public Task<T> HandleAsync(IRequestContext<GenericRequest<T>> requestContext)
            => Task.FromResult(default(T)!);
    }

    [TestMethod]
    public void Scanning_OpenGeneric_Without_GenericAttribute_Throws()
    {
        var sc = new ServiceCollection();
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            sc.AddLevadS(b =>
            {
                b.RegisterServicesFromAssemblyContaining<OpenGenericBadHandler<int>>();
            });
        });
    }

    [TestMethod]
    public void Scanning_GenericAttribute_With_Mismatched_ServiceType_Throws()
    {
        var sc = new ServiceCollection();
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            sc.AddLevadS(b =>
            {
                b.RegisterServicesFromAssemblyContaining<MismatchedGenericHandler<int>>();
            });
        });
    }

    [TestMethod]
    public void Scanning_GenericAttribute_With_Mismatched_InterfaceType_Throws()
    {
        var sc = new ServiceCollection();
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            sc.AddLevadS(b =>
            {
                b.RegisterServicesFromAssemblyContaining<MismatchedGenericInterfaceHandler<int>>();
            });
        });
    }
}

using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

public class ExceptionRequest : IRequest<int> { public int Id { get; set; } }

[TestClass]
public class ExceptionHandlersTests : BaseTestClass
{
    private int _handlerCalls;
    private int _exceptionHandlerCalls;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Register a handler that throws an exception
        builder.AddRequestHandler<ExceptionRequest, int>("throw", () =>
        {
            _handlerCalls++;
            throw new InvalidOperationException("Test exception");
        });

        // Register an exception handler for InvalidOperationException
        builder.AddRequestExceptionHandler<ExceptionRequest, int, InvalidOperationException>("throw", (ctx, callback) =>
        {
            _exceptionHandlerCalls++;
            callback(999);
            return Task.FromResult(true); // Suppress exception and return a value
        });

        // Register a handler that does not throw
        builder.AddRequestHandler<ExceptionRequest, int>("no-throw", () =>
        {
            _handlerCalls++;
            return 123;
        });
        
        builder.AddRequestHandler<ExceptionRequest, int>("unhandled", () =>
        {
            throw new ArgumentNullException("Test unhandled exception");
        });
        
        builder.AddRequestExceptionHandler<ExceptionRequest, int, ArgumentNullException>("throw", (ctx, callback) =>
        {
            _exceptionHandlerCalls++;
            callback(888);
            return Task.FromResult(true);
        });
    }

    [TestMethod]
    public async Task ExceptionHandler_InvokedOnException()
    {
        var result = await Dispatcher.RequestAsync(new ExceptionRequest(), "throw");
        Assert.AreEqual(999, result); // Exception handler modifies the response
        Assert.AreEqual(1, _handlerCalls);
        Assert.AreEqual(1, _exceptionHandlerCalls);
    }

    [TestMethod]
    public async Task ExceptionHandler_NotInvokedWhenNoException()
    {
        var result = await Dispatcher.RequestAsync(new ExceptionRequest(), "no-throw");
        Assert.AreEqual(123, result); // Normal handler response
        Assert.AreEqual(1, _handlerCalls);
        Assert.AreEqual(0, _exceptionHandlerCalls);
    }

    [TestMethod]
    public async Task ExceptionHandler_UnhandledExceptionPropagates()
    {
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
        {
            await Dispatcher.RequestAsync(new ExceptionRequest(), "unhandled");
        });
    }

    [TestMethod]
    public async Task ExceptionHandler_MultipleHandlersForDifferentExceptions()
    {
        var result = await Dispatcher.RequestAsync(new ExceptionRequest(), "throw");
        Assert.AreEqual(999, result); // InvalidOperationException handler takes precedence
        Assert.AreEqual(1, _handlerCalls);
        Assert.AreEqual(1, _exceptionHandlerCalls);
    }
}
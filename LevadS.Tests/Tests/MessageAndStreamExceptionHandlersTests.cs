using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

// public class ExceptionMessage : IMessage { public string Content { get; set; } }
// public class ExceptionStreamRequest : IStreamRequest<int> { public int Id { get; set; } }
//
// [TestClass]
// public class MessageAndStreamExceptionHandlersTests : BaseTestClass
// {
//     private int _messageHandlerCalls;
//     private int _messageExceptionHandlerCalls;
//     private int _streamHandlerCalls;
//     private int _streamExceptionHandlerCalls;
//
//     [TestInitialize]
//     public override void Initialize() => base.Initialize();
//
//     [TestCleanup]
//     public override void Cleanup() => base.Cleanup();
//
//     protected override void InitializeLevadS(ILevadSBuilder builder)
//     {
//         // Message Handlers
//         builder.AddMessageHandler<ExceptionMessage>("throw", _ =>
//         {
//             _messageHandlerCalls++;
//             throw new InvalidOperationException("Test message exception");
//         });
//
//         builder.AddMessageExceptionHandler<ExceptionMessage, InvalidOperationException>("throw", (ctx, ex) =>
//         {
//             _messageExceptionHandlerCalls++;
//             return Task.CompletedTask; // Suppress exception
//         });
//
//         builder.AddMessageHandler<ExceptionMessage>("no-throw", _ =>
//         {
//             _messageHandlerCalls++;
//             return Task.CompletedTask;
//         });
//
//         // Stream Handlers
//         builder.AddStreamHandler<ExceptionStreamRequest, int>("throw", async _ =>
//         {
//             _streamHandlerCalls++;
//             throw new InvalidOperationException("Test stream exception");
//         });
//
//         builder.AddStreamExceptionHandler<ExceptionStreamRequest, int, InvalidOperationException>("throw", (ctx, ex) =>
//         {
//             _streamExceptionHandlerCalls++;
//             return Task.FromResult(Enumerable.Empty<int>().ToAsyncEnumerable()); // Suppress exception
//         });
//
//         builder.AddStreamHandler<ExceptionStreamRequest, int>("no-throw", async _ =>
//         {
//             _streamHandlerCalls++;
//             return new[] { 1, 2, 3 }.ToAsyncEnumerable();
//         });
//     }
//
//     [TestMethod]
//     public async Task MessageExceptionHandler_InvokedOnException()
//     {
//         await Dispatcher.PublishAsync(new ExceptionMessage { Content = "Test" }, "throw");
//         Assert.AreEqual(1, _messageHandlerCalls);
//         Assert.AreEqual(1, _messageExceptionHandlerCalls);
//     }
//
//     [TestMethod]
//     public async Task MessageExceptionHandler_NotInvokedWhenNoException()
//     {
//         await Dispatcher.PublishAsync(new ExceptionMessage { Content = "Test" }, "no-throw");
//         Assert.AreEqual(1, _messageHandlerCalls);
//         Assert.AreEqual(0, _messageExceptionHandlerCalls);
//     }
//
//     [TestMethod]
//     public async Task StreamExceptionHandler_InvokedOnException()
//     {
//         var result = await Dispatcher.StreamAsync(new ExceptionStreamRequest { Id = 1 }, "throw").ToListAsync();
//         Assert.AreEqual(0, result.Count); // Exception handler suppresses output
//         Assert.AreEqual(1, _streamHandlerCalls);
//         Assert.AreEqual(1, _streamExceptionHandlerCalls);
//     }
//
//     [TestMethod]
//     public async Task StreamExceptionHandler_NotInvokedWhenNoException()
//     {
//         var result = await Dispatcher.StreamAsync(new ExceptionStreamRequest { Id = 1 }, "no-throw").ToListAsync();
//         CollectionAssert.AreEqual(new[] { 1, 2, 3 }, result);
//         Assert.AreEqual(1, _streamHandlerCalls);
//         Assert.AreEqual(0, _streamExceptionHandlerCalls);
//     }
// }
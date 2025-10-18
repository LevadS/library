using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Tests.Tests;

[TestClass]
public class FromTopicNegativeTests : BaseTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    private sealed class R : IRequest<int> { }

    protected override void InitializeLevadS(ILevadSBuilder builder)
    {
        // Handler delegate with [FromTopic] param
        builder.AddRequestHandler<R, int>("ft:{n:int}", ([FromTopic("n")] int n) => n);

        // A filter with [FromTopic] via wrapper
        builder.AddRequestHandler<R, int>("ff:{x:int}", () => 0)
               .WithFilter<TopicReadingFilter>("ff:{x:int}");
    }

    private sealed class TopicReadingFilter : IRequestHandlingFilter<R, int>
    {
        public Task<int> InvokeAsync(IRequestContext<R> requestContext, RequestHandlingFilterNextDelegate<int> next)
            => next();

        public TopicReadingFilter([FromTopic("x")] int x)
        {
            // ctor will get x from topic
        }
    }

    [TestMethod]
    public async Task FromTopic_MissingValue_Throws()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            // Pattern requires ft:{n:int}, but no capture provided
            await Dispatcher.RequestAsync(new R(), "ft");
        });
    }

    [TestMethod]
    public async Task FromTopic_ConversionFailure_Throws()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            // n must be int, provide non-numeric
            await Dispatcher.RequestAsync(new R(), "ft:not-an-int");
        });
    }

    [TestMethod]
    public async Task FromTopic_FilterCtor_Missing_Throws()
    {
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
            // ff:{x:int} requires x; missing should fail during filter creation
            await Dispatcher.RequestAsync(new R(), "ff");
        });
    }
}

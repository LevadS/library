namespace LevadS.Classes.Builders;

internal abstract class OutletDescriptor(Type messageType, params string[] topicPatterns)
{
    public Type MessageType { get; } = messageType;
    public List<string> TopicPatterns { get; } = [..topicPatterns];

    public abstract Task<IAsyncDisposable> SetupAsync(ILevada levada);
}

internal class OutletDescriptor<TMessage>(params string[] topicPatterns)
    : OutletDescriptor(typeof(TMessage), topicPatterns)
{
    private readonly string[] _topicPatterns = topicPatterns;

    public override async Task<IAsyncDisposable> SetupAsync(ILevada levada)
        => new DisposableContainer(
            await Task.WhenAll(_topicPatterns.Select(levada.RegisterOutletAsync<TMessage>))
        );
}
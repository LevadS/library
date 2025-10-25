namespace LevadS.Classes.Builders;

internal abstract class InletDescriptor(Type messageType, params string[] topicPatterns)
{
    public Type MessageType { get; } = messageType;
    public List<string> TopicPatterns { get; } = [..topicPatterns];

    public abstract Task<IAsyncDisposable> SetupAsync(ILevada levada);
}

internal class InletDescriptor<TMessage>(params string[] topicPatterns)
    : InletDescriptor(typeof(TMessage), topicPatterns)
{
    private readonly string[] _topicPatterns = topicPatterns;

    public override async Task<IAsyncDisposable> SetupAsync(ILevada levada)
        => new AsyncDisposableContainer(
            await Task.WhenAll(_topicPatterns.Select(levada.RegisterInletAsync<TMessage>))
        );
}
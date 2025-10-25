namespace LevadS.Classes.Builders;

public sealed class DisposableContainer(params IDisposable[] disposables) : IDisposable, IAsyncDisposable
{
    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }
}
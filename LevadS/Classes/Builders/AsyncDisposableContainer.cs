namespace LevadS.Classes.Builders;

public sealed class AsyncDisposableContainer(params IAsyncDisposable[] disposables) : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
        => await Task.WhenAll(disposables.Select(disposable => disposable.DisposeAsync().AsTask()));
}
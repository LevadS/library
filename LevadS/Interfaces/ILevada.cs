namespace LevadS;

internal interface ILevada
{
    Task CheckConfigurationAsync();
    Task<IAsyncDisposable> RegisterInletAsync<TMessage>(string topicPattern);
    Task<IAsyncDisposable> RegisterOutletAsync<TMessage>(string topicPattern);
}

internal interface ILevadaService : ILevada
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
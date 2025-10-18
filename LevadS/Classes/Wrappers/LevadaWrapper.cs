using System.Diagnostics;
using LevadS.Interfaces;
using LevadS.Classes.Builders;

namespace LevadS.Classes;

internal class LevadaWrapper<TLevada>(IServiceProvider serviceProvider, TLevada levada, LevadaBuilder<TLevada> levadaBuilder) : ILevadaService
    where TLevada : ILevada
{
    private TLevada _levada = levada;

    public Task CheckConfigurationAsync()
        => _levada.CheckConfigurationAsync();

    public Task<IAsyncDisposable> RegisterInletAsync<TMessage>(string topicPattern)
        => _levada.RegisterInletAsync<TMessage>(topicPattern);

    public Task<IAsyncDisposable> RegisterOutletAsync<TMessage>(string topicPattern)
        => _levada.RegisterOutletAsync<TMessage>(topicPattern);
    
    private readonly List<IAsyncDisposable> _disposables = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Debug.WriteLine($"Starting levada {_levada.GetType().Name}");
        
        var task = levadaBuilder.ConfigurationAction?.Invoke(serviceProvider, _levada);
        if (task is not null)
        {
            await task;
        }

        _disposables.AddRange(
            await Task.WhenAll(levadaBuilder.Inlets.Select(d => d.SetupAsync(_levada)))
        );

        _disposables.AddRange(
            await Task.WhenAll(levadaBuilder.Outlets.Select(d => d.SetupAsync(_levada)))
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.WhenAll(_disposables.Select(disposable => disposable.DisposeAsync().AsTask()));
}
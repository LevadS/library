using Microsoft.Extensions.Hosting;

namespace LevadS.Services;

internal class LevadSHost(IDictionary<string, ILevadaService> levadaServices) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.WhenAll(levadaServices.Values.Select(s => s.StartAsync(cancellationToken)));

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.WhenAll(levadaServices.Values.Select(s => s.StopAsync(cancellationToken)));
}
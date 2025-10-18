using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LevadS.Tests;

public abstract class BaseTestClass
{
    private IServiceCollection serviceCollection;
    private IServiceCollection ServiceCollection => serviceCollection ??= new ServiceCollection();

    private IServiceProvider serviceProvider;
    protected IServiceProvider ServiceProvider => serviceProvider ??= ServiceCollection.BuildServiceProvider();

    private TestingHost testingHost;
    private TestingHost TestingHost => testingHost ??= ServiceProvider.GetRequiredService<TestingHost>();
        
    protected IDispatcher Dispatcher => ServiceProvider.GetRequiredService<IDispatcher>();

    public virtual void Initialize()
    {
        ServiceCollection.AddLevadS(b => InitializeLevadS(b));
        ServiceCollection.AddSingleton<TestingHost>();
        ServiceCollection.AddSingleton<IHostApplicationLifetime>(services => services.GetRequiredService<TestingHost>());
        ServiceCollection.AddLogging(builder => builder.AddConsole());

        var hostedServices = ServiceProvider.GetRequiredService<IEnumerable<IHostedService>>();
        Task.WaitAll(hostedServices.Select(s => s.StartAsync(new CancellationToken())).ToArray());

        TestingHost.StartApplication();
    }

    public virtual void Cleanup()
    {
        TestingHost.StopApplication();

        serviceCollection = null;
        serviceProvider = null;
    }

    protected abstract void InitializeLevadS(ILevadSBuilder builder);
}
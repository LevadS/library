using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Benchmarks.Classes;

public abstract class BenchmarkBase
{
    private IServiceCollection ServiceCollection { get; }
    
    protected IServiceProvider ServiceProvider { get; }
    
    public BenchmarkBase()
    {
        ServiceCollection = new ServiceCollection();
        
        ConfigureServices(ServiceCollection);

        ServiceCollection.AddLevadS(ConfigureLevadS);

        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }
    
    public virtual void ConfigureServices(IServiceCollection serviceCollection) {}
    
    public virtual void ConfigureLevadS(ILevadSBuilder builder) {}
}
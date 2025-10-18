using LevadS.Interfaces;

namespace LevadS.Classes.Builders;

internal class LevadaBuilder<TLevada> : ILevadaBuilder<TLevada>, IConfiguredLevadaBuilder
    where TLevada : ILevada
{
    public Func<IServiceProvider, TLevada, Task>? ConfigurationAction { get; private set; }
    
    public List<InletDescriptor> Inlets { get; } = [];
    public List<OutletDescriptor> Outlets { get; } = [];

    public IConfiguredLevadaBuilder Configure(Action<IServiceProvider, TLevada> configurationAction)
    {
        ConfigurationAction = (sp, l) =>
        {
            configurationAction(sp, l);
            
            return Task.CompletedTask;
        };

        return this;
    }

    public IConfiguredLevadaBuilder Configure(Func<IServiceProvider, TLevada, Task> configurationActionAsync)
    {
        ConfigurationAction = configurationActionAsync;
        
        return this;
    }

    public IConfiguredLevadaBuilder AddInlet<TMessage>(params string[] topicPatterns)
    {
        Inlets.Add(new InletDescriptor<TMessage>(topicPatterns));
        
        return this;
    }

    public IConfiguredLevadaBuilder AddOutlet<TMessage>(params string[] topicPatterns)
    {
        Outlets.Add(new OutletDescriptor<TMessage>(topicPatterns));
        
        return this;
    }
}
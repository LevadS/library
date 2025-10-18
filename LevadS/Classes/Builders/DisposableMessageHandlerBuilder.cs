using Microsoft.Extensions.DependencyInjection;
using LevadS.Interfaces;

namespace LevadS.Classes.Builders;

internal class DisposableMessageHandlerBuilder<TMessage>(
    ILevadSBuilder levadSBuilder,
    IServiceCollection serviceCollection,
    string key,
    IEnumerable<ServiceDescriptor> serviceDescriptors,
    IServiceManager serviceManager
)
    : MessageHandlerBuilder<TMessage>(levadSBuilder, serviceCollection, key), IDisposableMessageHandlerBuilder<TMessage>
{
    private readonly HandlerDescriptor _descriptor = new(serviceManager, serviceDescriptors);
    private readonly IServiceCollection _serviceCollection = serviceCollection;

    public IAsyncDisposable Build()
    {
        serviceManager.UpdateServices(_serviceCollection);
        return _descriptor;
    }

    public override IDisposableMessageHandlerBuilder<TMessage> WithFilter<TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        base.WithFilter(topicPattern, filterFactory);
        
        _descriptor.ServiceDescriptors.AddRange(_serviceCollection.TakeLast(1));
        
        return this;
    }
}
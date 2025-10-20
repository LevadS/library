using Microsoft.Extensions.DependencyInjection;
using LevadS.Interfaces;

namespace LevadS.Classes.Builders;

internal class DisposableStreamHandlerBuilder<TRequest, TResponse>(
    ILevadSBuilder levadSBuilder,
    IServiceCollection serviceCollection,
    string key,
    IEnumerable<ServiceDescriptor> serviceDescriptors,
    IServiceManager serviceManager
)
    : StreamHandlerBuilder<TRequest, TResponse>(levadSBuilder, serviceCollection, key), IDisposableStreamHandlerBuilder<TRequest, TResponse>
{
    private readonly HandlerDescriptor _descriptor = new(serviceManager, serviceDescriptors);
    private readonly IServiceCollection _serviceCollection = serviceCollection;

    public IAsyncDisposable Build()
    {
        serviceManager.UpdateServices(_serviceCollection);
        return _descriptor;
    }
    
    public override IDisposableStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        base.WithFilter(topicPattern, filterFactory);
        
        _descriptor.ServiceDescriptors.AddRange(_serviceCollection.TakeLast(1));
        
        return this;
    }
}
using Microsoft.Extensions.DependencyInjection;
using LevadS.Interfaces;

namespace LevadS.Classes.Builders;

internal class DisposableRequestHandlerBuilder<TRequest, TResponse>(
    ILevadSBuilder levadSBuilder,
    IServiceCollection serviceCollection,
    string key,
    IEnumerable<ServiceDescriptor> serviceDescriptors,
    IServiceManager serviceManager
)
    : RequestHandlerBuilder<TRequest, TResponse>(levadSBuilder, serviceCollection, key), IDisposableRequestHandlerBuilder<TRequest, TResponse>
{
    private readonly HandlerDescriptor _descriptor = new(serviceManager, serviceDescriptors);
    private readonly IServiceCollection _serviceCollection = serviceCollection;

    public IAsyncDisposable Build()
    {
        serviceManager.UpdateServices(_serviceCollection);
        return _descriptor;
    }
    
    public override IDisposableRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        base.WithFilter(topicPattern, filterFactory);
        
        _descriptor.ServiceDescriptors.AddRange(_serviceCollection.TakeLast(1));
        
        return this;
    }
}
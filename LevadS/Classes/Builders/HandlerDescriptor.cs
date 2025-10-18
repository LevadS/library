using Microsoft.Extensions.DependencyInjection;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class HandlerDescriptor(IServiceManager serviceManager, IEnumerable<ServiceDescriptor> serviceDescriptors) : IAsyncDisposable, IDisposable
{
    public List<ServiceDescriptor> ServiceDescriptors { get; } = serviceDescriptors.ToList();

    public ValueTask DisposeAsync()
    {
        serviceManager.RemoveServices(ServiceDescriptors);

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        serviceManager.RemoveServices(ServiceDescriptors);
    }
}
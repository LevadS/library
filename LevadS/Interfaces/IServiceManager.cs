using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

internal interface IServiceManager
{
    void UpdateServices(IServiceCollection newServiceCollection);
    void RemoveServices(IEnumerable<ServiceDescriptor> serviceDescriptors);
}
using LevadS.Services;
using LevadS.Interfaces;
using LevadS.Classes.Builders;
using LevadS.Classes.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS;

public static class DependencyInjection
{
    /// <summary>
    /// Registers LevadS services and provides handler registration API.
    /// </summary>
    /// <param name="serviceCollection">Extended service collection</param>
    /// <param name="builder">Builder action that exposes handler registration API</param>
    /// <returns>Extended service collection</returns>
    public static IServiceCollection AddLevadS(this IServiceCollection serviceCollection, Action<ILevadSBuilder> builder)
    {
        var servicesContainer = new ServiceContainer();
        serviceCollection
            .AddSingleton(servicesContainer)
            .AddSingleton<IServiceRegister>(servicesContainer)
            .AddSingleton<IServiceResolver>(servicesContainer)
            .AddRuntimeRegistrationServices()
            .AddHostedService(serviceProvider =>
            {
                servicesContainer.ServiceProvider = serviceProvider;
                return servicesContainer;
            });
        
        builder.Invoke(new LevadSBuilder(servicesContainer));
        
        return serviceCollection
            .AddSingleton<IDispatcher, Dispatcher>()
            
            // .AddHostedService<LevadSHost>()
            ;
    }

    internal static IServiceCollection AddRuntimeRegistrationServices(this IServiceCollection serviceCollection)
    {
        if (serviceCollection.Any(d => d.ServiceType == typeof(IHandlersRegister)))
        {
            return serviceCollection;
        }

        // Register LevadSBuilder as the single implementation instance and map all runtime registration interfaces
        serviceCollection.AddSingleton<LevadSBuilder>();

        var runtimeInterfaces = new[]
        {
            typeof(IHandlersRegister), typeof(IMessageHandlersRegister), typeof(IRequestHandlersRegister), typeof(IStreamHandlersRegister),
            typeof(IFiltersRegister), typeof(IMessageFiltersRegister), typeof(IRequestFiltersRegister), typeof(IStreamFiltersRegister),
            typeof(IDispatchFiltersRegister), typeof(IMessageDispatchFiltersRegister), typeof(IRequestDispatchFiltersRegister), typeof(IStreamDispatchFiltersRegister),
            typeof(IExceptionHandlersRegister), typeof(IMessageExceptionHandlersRegister), typeof(IRequestExceptionHandlersRegister), typeof(IStreamExceptionHandlersRegister),
            typeof(IMessageServicesRegister), typeof(IRequestServicesRegister), typeof(IStreamServicesRegister)
        };

        foreach (var iface in runtimeInterfaces)
        {
            serviceCollection.AddSingleton(iface, sp => sp.GetRequiredService<LevadSBuilder>());
        }

        return serviceCollection;
    }
}
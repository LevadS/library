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

        return serviceCollection
            .AddSingleton<IHandlersRegister, LevadSBuilder>()
            .AddSingleton<IMessageHandlersRegister, LevadSBuilder>()
            .AddSingleton<IRequestHandlersRegister, LevadSBuilder>()
            .AddSingleton<IStreamHandlersRegister, LevadSBuilder>()

            .AddSingleton<IFiltersRegister, LevadSBuilder>()
            .AddSingleton<IMessageFiltersRegister, LevadSBuilder>()
            .AddSingleton<IRequestFiltersRegister, LevadSBuilder>()
            .AddSingleton<IStreamFiltersRegister, LevadSBuilder>()

            .AddSingleton<IDispatchFiltersRegister, LevadSBuilder>()
            .AddSingleton<IMessageDispatchFiltersRegister, LevadSBuilder>()
            .AddSingleton<IRequestDispatchFiltersRegister, LevadSBuilder>()
            .AddSingleton<IStreamDispatchFiltersRegister, LevadSBuilder>()

            .AddSingleton<IExceptionHandlersRegister, LevadSBuilder>()
            .AddSingleton<IMessageExceptionHandlersRegister, LevadSBuilder>()
            .AddSingleton<IRequestExceptionHandlersRegister, LevadSBuilder>()
            .AddSingleton<IStreamExceptionHandlersRegister, LevadSBuilder>()
            
            .AddSingleton<IMessageServicesRegister, LevadSBuilder>()
            .AddSingleton<IRequestServicesRegister, LevadSBuilder>()
            .AddSingleton<IStreamServicesRegister, LevadSBuilder>()
        ;
    }
}
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
        builder.Invoke(new LevadSBuilder(serviceCollection));
        
        return serviceCollection
            .AllowResolvingKeyedServicesAsDictionary()
            .AddSingleton<IDispatcher>(p => new Dispatcher(serviceCollection, p))
            
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
            .AddSingleton<IHandlersRegister, ServicesRegister>()
            .AddSingleton<IMessageHandlersRegister, ServicesRegister>()
            .AddSingleton<IRequestHandlersRegister, ServicesRegister>()
            .AddSingleton<IStreamHandlersRegister, ServicesRegister>()

            .AddSingleton<IFiltersRegister, ServicesRegister>()
            .AddSingleton<IMessageFiltersRegister, ServicesRegister>()
            .AddSingleton<IRequestFiltersRegister, ServicesRegister>()
            .AddSingleton<IStreamFiltersRegister, ServicesRegister>()

            .AddSingleton<IDispatchFiltersRegister, ServicesRegister>()
            .AddSingleton<IMessageDispatchFiltersRegister, ServicesRegister>()
            .AddSingleton<IRequestDispatchFiltersRegister, ServicesRegister>()
            .AddSingleton<IStreamDispatchFiltersRegister, ServicesRegister>()

            .AddSingleton<IExceptionHandlersRegister, ServicesRegister>()
            .AddSingleton<IMessageExceptionHandlersRegister, ServicesRegister>()
            .AddSingleton<IRequestExceptionHandlersRegister, ServicesRegister>()
            .AddSingleton<IStreamExceptionHandlersRegister, ServicesRegister>()
            
            .AddSingleton<IMessageServicesRegister, ServicesRegister>()
            .AddSingleton<IRequestServicesRegister, ServicesRegister>()
            .AddSingleton<IStreamServicesRegister, ServicesRegister>()
        ;
    }
}
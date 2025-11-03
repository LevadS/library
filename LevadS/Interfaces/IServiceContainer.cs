using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Interfaces;

public interface IServiceRegister
{
    public IDisposable Register(
        Type serviceType,
        Type implementationType,
        Type inputType,
        Type outputType,
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string? key = null
    );

    public IDisposable Register<TService, TImplementation, TInput, TOutput>(
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        Func<IContext, TImplementation>? factory = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string? key = null
    )
        where TImplementation : class, TService;

    public IDisposable Register<TService, TImplementation, TInput>(
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        Func<IContext, TImplementation>? factory = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string? key = null
    )
        where TImplementation : class, TService;
}

public interface IServiceResolver : IDisposable, IAsyncDisposable
{
    IServiceProvider ServiceProvider { get; }
    
    IServiceResolver CreateScope();

    IEnumerable<(TService, IContext)> GetServices<TService, TRequestedInput, TRequestedOutput>(IContext context, string[]? keys = null);
    
    IEnumerable<(object, IContext)> GetServices<TRequestedInput, TRequestedOutput>(Type serviceType, IContext context, string[]? keys = null);

    (TService, IContext)? GetService<TService, TRequestedInput, TRequestedOutput>(IContext context, string[]? keys = null)
        => GetServices<TService, TRequestedInput, TRequestedOutput>(context, keys).FirstOrDefault();

    (object, IContext)? GetService<TRequestedInput, TRequestedOutput>(Type serviceType, IContext context, string[]? keys = null)
        => GetServices<TRequestedInput, TRequestedOutput>(serviceType, context, keys).FirstOrDefault();
}
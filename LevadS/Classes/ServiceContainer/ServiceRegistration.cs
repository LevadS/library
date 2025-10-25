using LevadS.Classes;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS;

internal abstract class ServiceRegistration
{
    internal Type ServiceType { get; set; }
    internal Type GenericTypeDefinition { get; set; }
    internal Type InputType { get; set; }
    internal Type? OutputType { get; set; }
    internal string TopicPattern { get; set; }
    internal ServiceLifetime Lifetime { get; set; }
    internal string? Key { get; set; }
    internal bool IsVariant { get; set; }

    internal abstract void DisposeSingletonIfNeeded(ServiceContainer container);
    internal abstract object GetOrCreateSingleton<TInput, TOutput>(ServiceContainer container, IContext context);
    internal abstract object CreateInstance<TInput, TOutput>(IContext context);
}

internal class ServiceRegistration<TInput, TOutput> : ServiceRegistration
{
    private readonly Func<IContext, object> _factory;
    private object? _singletonInstance;
    private bool _singletonInitialized;
    private readonly object _singletonLock = new();

    public ServiceRegistration(
        Type serviceType,
        IServiceEnveloper enveloper,
        string topicPattern,
        ServiceLifetime lifetime,
        Func<IContext, object> factory,
        object? singletonInstance,
        string? key
    )
    {
        ServiceType = serviceType;
        IsVariant = ServiceType.IsAssignableTo(typeof(IFilter));
        GenericTypeDefinition = ServiceType.GetGenericTypeDefinition();
        InputType = typeof(TInput);
        OutputType = typeof(TOutput);
        Enveloper = enveloper;
        TopicPattern = topicPattern;
        Lifetime = lifetime;
        _factory = factory;
        Key = key;

        if (singletonInstance != null)
        {
            _singletonInstance = singletonInstance;
            _singletonInitialized = true;
        }
    }

    internal IServiceEnveloper Enveloper { get; }

    internal override object CreateInstance<TRequestedInput, TRequestedOutput>(IContext context)
        => Enveloper.Envelop<TInput, TRequestedInput, TOutput, TRequestedOutput>(_factory(context));

    internal override object GetOrCreateSingleton<TRequestedInput, TRequestedOutput>(ServiceContainer container, IContext context)
    {
        if (_singletonInitialized)
        {
            return _singletonInstance!;
        }

        lock (_singletonLock)
        {
            if (!_singletonInitialized)
            {
                ((Context)context).ServiceProvider = container.ServiceProvider;
                var instance = CreateInstance<TRequestedInput, TRequestedOutput>(context);
                _singletonInstance = instance;
                _singletonInitialized = true;
                container.TrackSingletonDisposable(instance);
            }
        }

        return _singletonInstance!;
    }

    internal override void DisposeSingletonIfNeeded(ServiceContainer container)
    {
        object? instance = null;
        lock (_singletonLock)
        {
            if (_singletonInitialized)
            {
                instance = _singletonInstance;
                _singletonInstance = null;
                _singletonInitialized = false;
            }
        }

        if (instance == null)
        {
            return;
        }

        container.UntrackSingletonDisposable(instance);

        switch (instance)
        {
            case IAsyncDisposable asyncDisposable:
                asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}
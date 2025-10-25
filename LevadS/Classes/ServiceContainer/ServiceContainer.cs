using System.Collections.ObjectModel;
using LevadS.Classes;
using LevadS.Classes.Extensions;
using LevadS.Extensions;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LevadS;

internal sealed class ServiceContainer : IServiceRegister, IServiceResolver, IHostedService
{
    private IServiceProvider _rootServiceProvider;
    public IServiceProvider ServiceProvider
    {
        get => _rootServiceProvider;
        set
        {
            _rootServiceProvider = value;
            _scopeFactory = _rootServiceProvider.GetService<IServiceScopeFactory>()
                            ?? throw new InvalidOperationException("The provided IServiceProvider does not expose an IServiceScopeFactory.");
            _rootContainerScope = new ServiceContainerScope(this, _rootServiceProvider, null, ownsScope: false);
        }
    }
    private IServiceScopeFactory _scopeFactory;
    private readonly object _registrationLock = new();
    private readonly List<IDisposable> _singletonDisposables = new();
    private readonly List<IAsyncDisposable> _singletonAsyncDisposables = new();
    private ServiceRegistration[] _registrations = Array.Empty<ServiceRegistration>();
    private ServiceContainerScope _rootContainerScope;
    private bool _disposed;

    public IDisposable Register(
        Type serviceType,
        Type implementationType,
        Type inputType,
        Type? outputType,
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string? key = null
    )
        => RegisterCore(
            topicPattern,
            lifetime,
            serviceType,
            inputType,
            outputType,
            enveloper,
            ctx => ctx.ServiceProvider.CreateInstanceWithTopic(implementationType, ctx),
            key: key
        );

    public IDisposable Register<TService, TImplementation, TInput, TOutput>(
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        Func<IContext, TImplementation>? factory = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string? key = null
    )
        where TImplementation : class, TService
        => RegisterCore(
            topicPattern,
            lifetime,
            typeof(TService),
            typeof(TInput),
            typeof(TOutput),
            enveloper,
            factory ?? (ctx => ctx.ServiceProvider.CreateInstanceWithTopic<TImplementation>(ctx)),
            key: key
        );

    public IDisposable Register<TService, TImplementation, TInput>(
        IServiceEnveloper enveloper,
        string topicPattern = "*",
        Func<IContext, TImplementation>? factory = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient,
        string? key = null
    )
        where TImplementation : class, TService
        => RegisterCore(
            topicPattern,
            lifetime,
            typeof(TService),
            typeof(TInput),
            typeof(object),
            enveloper,
            factory ?? (ctx => ctx.ServiceProvider.CreateInstanceWithTopic<TImplementation>(ctx)),
            key: key
        );

    // public IDisposable Register<TService>(string topicPattern, Func<IServiceProvider, string, string, IReadOnlyDictionary<string, object>, Context> contextFactory, ServiceLifetime lifetime = ServiceLifetime.Transient, string? key = null)
    //     where TService : class
    //     => Register<TService, TService>(topicPattern, contextFactory, lifetime, key);

    // public IDisposable Register<TService, TImplementation>(string topicPattern, Func<IServiceProvider, string, string, IReadOnlyDictionary<string, object>, Context> contextFactory, Func<IContext, TImplementation>? factory = null, ServiceLifetime lifetime = ServiceLifetime.Transient, string? key = null)
    //     where TImplementation : class, TService
    // {
    //     factory ??= ctx => ctx.ServiceProvider.CreateInstanceWithTopic<TImplementation>(ctx);
    //     return RegisterCore(topicPattern, lifetime, typeof(TService), factory, contextFactory, key);
    // }

    // public IDisposable Register<TService>(string topicPattern,
    //     Func<IServiceProvider, string, string, IReadOnlyDictionary<string, object>, Context> contextFactory,
    //     Func<IContext, TService>? factory = null, ServiceLifetime lifetime = ServiceLifetime.Transient, string? key = null)
    //     where TService : class
    // {
    //     factory ??= ctx => ctx.ServiceProvider.CreateInstanceWithTopic<TService>(ctx);
    //     return Register<TService, TService>(topicPattern, contextFactory, factory, lifetime, key);
    // }

    // public IDisposable RegisterInstance<TService, TImplementation>(string topicPattern, TImplementation instance, string? key = null)
    //     where TImplementation : class, TService
    // {
    //     ArgumentNullException.ThrowIfNull(instance);
    //     return RegisterCore(topicPattern, ServiceLifetime.Singleton, typeof(TService), _ => instance, singletonInstance: instance, key: key);
    // }
    //
    // public IDisposable RegisterInstance<TService>(string topicPattern, TService instance, string? key = null)
    //     where TService : class
    //     => RegisterInstance<TService, TService>(topicPattern, instance, key);

    public IServiceResolver CreateScope()
        => _rootContainerScope.CreateScope();

    public IEnumerable<(TService, IContext)> GetServices<TService, TRequestedInput, TRequestedOutput>(IContext context, string[]? keys = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        return GetServices<TService, TRequestedInput, TRequestedOutput>(_rootContainerScope, context, keys);
    }

    public IEnumerable<(object, IContext)> GetServices<TRequestedInput, TRequestedOutput>(Type serviceType, IContext context, string[]? keys = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        return GetServices<TRequestedInput, TRequestedOutput>(serviceType, _rootContainerScope, context, keys);
    }

    internal IEnumerable<(object, IContext)> GetServices<TRequestedInput, TRequestedOutput>(Type serviceType, ServiceContainerScope containerScope, IContext context, string[]? keys = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(containerScope);
        ArgumentNullException.ThrowIfNull(context);
        containerScope.EnsureNotDisposed();

        foreach (var result in GetResolutionsInternal<TRequestedInput, TRequestedOutput>(serviceType, containerScope, context, keys))
        {
            yield return result;
        }
    }

    internal IEnumerable<(TService, IContext)> GetServices<TService, TRequestedInput, TRequestedOutput>(ServiceContainerScope containerScope, IContext context, string[]? keys = null)
        => GetServices<TRequestedInput, TRequestedOutput>(typeof(TService), containerScope, context, keys).Select(t => ((TService)t.Item1, t.Item2));

    public (TService, IContext)? GetService<TService, TRequestedInput, TRequestedOutput>(IContext context,  string[]? keys = null)
        => GetServices<TService, TRequestedInput, TRequestedOutput>(context, keys).FirstOrDefault();

    internal (TService, IContext)? GetService<TService, TRequestedInput, TRequestedOutput>(ServiceContainerScope containerScope, Context context,  string[]? keys = null)
        => GetServices<TService, TRequestedInput, TRequestedOutput>(containerScope, context, keys).FirstOrDefault();

    // public IEnumerable<TopicServiceResolution<TService>> GetServiceResolutions<TService>(string topic, string? key = null)
    //     => GetServiceResolutions<TService>(_rootScope, topic, key);

    // public IEnumerable<TopicServiceResolution<TService>> GetServiceResolutions<TService>(TopicServiceScope scope, string topic, string? key = null)
    // {
    //     ThrowIfDisposed();
    //     ArgumentNullException.ThrowIfNull(scope);
    //     ArgumentNullException.ThrowIfNull(topic);
    //     scope.EnsureNotDisposed();
    //
    //     foreach (var result in GetResolutionsInternal(typeof(TService), scope, topic, key))
    //     {
    //         yield return new TopicServiceResolution<TService>((TService)result.Instance, result.TopicPattern, result.CapturedValues, result.Key);
    //     }
    // }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _rootContainerScope.DisposeAsync().ConfigureAwait(false);

        List<IAsyncDisposable> asyncSingletons;
        List<IDisposable> syncSingletons;
        lock (_registrationLock)
        {
            asyncSingletons = _singletonAsyncDisposables.ToList();
            syncSingletons = _singletonDisposables.ToList();
            _singletonAsyncDisposables.Clear();
            _singletonDisposables.Clear();
        }

        foreach (var asyncDisposable in asyncSingletons)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }

        foreach (var disposable in syncSingletons)
        {
            disposable.Dispose();
        }

        _registrations = Array.Empty<ServiceRegistration>();
    }

    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();

    private IDisposable RegisterCore(
        string topicPattern,
        ServiceLifetime lifetime,
        Type serviceType,
        Type inputType,
        Type outputType,
        IServiceEnveloper enveloper,
        Func<IContext, object> factory,
        object? singletonInstance = null,
        string? key = null
    )
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(topicPattern);
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(factory);

        if (!topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }

        var registrationType = typeof(ServiceRegistration<,>).MakeGenericType(inputType, outputType ?? typeof(object));
        var registration = (ServiceRegistration)Activator.CreateInstance(registrationType, serviceType, enveloper, topicPattern, lifetime, factory, singletonInstance, key);
        var handle = AddRegistration(registration);

        if (singletonInstance != null)
        {
            TrackSingletonDisposable(singletonInstance);
        }

        return handle;
    }

    private RegistrationHandle AddRegistration(ServiceRegistration registration)
    {
        lock (_registrationLock)
        {
            var current = _registrations;
            var updated = new ServiceRegistration[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[^1] = registration;
            Volatile.Write(ref _registrations, updated);
        }

        return new RegistrationHandle(this, registration);
    }

    private void RemoveRegistration(ServiceRegistration registration)
    {
        lock (_registrationLock)
        {
            var current = _registrations;
            var index = Array.IndexOf(current, registration);
            if (index < 0)
            {
                return;
            }

            ServiceRegistration[] snapshot;
            if (current.Length == 1)
            {
                snapshot = Array.Empty<ServiceRegistration>();
            }
            else
            {
                snapshot = new ServiceRegistration[current.Length - 1];
                if (index > 0)
                {
                    Array.Copy(current, 0, snapshot, 0, index);
                }

                if (index < current.Length - 1)
                {
                    Array.Copy(current, index + 1, snapshot, index, current.Length - index - 1);
                }
            }

            Volatile.Write(ref _registrations, snapshot);
        }

        registration.DisposeSingletonIfNeeded(this);

        _rootContainerScope.ReleaseRegistration(registration);
    }

    private sealed class RegistrationHandle : IDisposable
    {
        private readonly ServiceContainer _container;
        private ServiceRegistration? _registration;

        internal RegistrationHandle(ServiceContainer container, ServiceRegistration registration)
        {
            _container = container;
            _registration = registration;
        }

        public void Dispose()
        {
            var registration = Interlocked.Exchange(ref _registration, null);
            if (registration != null)
            {
                _container.RemoveRegistration(registration);
            }
        }
    }

    private IEnumerable<(object, IContext)> GetResolutionsInternal<TRequestedInput, TRequestedOutput>(Type serviceType, ServiceContainerScope containerScope, IContext context, string[]? keys)
    {
        var registrations = Volatile.Read(ref _registrations);
        foreach (var registration in registrations)
        {
            // if (registration.ServiceType != serviceType)
            // {
            //     continue;
            // }

            if (registration.IsVariant
                    ? serviceType.GetGenericTypeDefinition() != registration.GenericTypeDefinition ||
                        (
                            !typeof(TRequestedInput).IsAssignableTo(registration.InputType) &&
                            !typeof(TRequestedOutput).IsAssignableFrom(registration.OutputType)
                        )
                      : registration.ServiceType != serviceType
            )
            {
                continue;
            }

            if (keys != null && !keys.Any(key => string.Equals(registration.Key, key, StringComparison.Ordinal)))
            {
                continue;
            }

            var captures = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (!context.Topic.MatchesTopicPattern(registration.TopicPattern, captures))
            {
                continue;
            }

            context = ((Context)context).CloneInstance();
            ((Context)context).CapturedValues = new ReadOnlyDictionary<string, object>(captures);
            ((Context)context).Key = registration.Key;
            var instance = containerScope.Resolve<TRequestedInput, TRequestedOutput>(registration, context);
            yield return (instance, context);//new ResolutionResult(instance, registration.TopicPattern, context.CapturedValues, registration.Key);
        }
    }

    internal void TrackSingletonDisposable(object instance)
    {
        lock (_registrationLock)
        {
            switch (instance)
            {
                case IAsyncDisposable asyncDisposable:
                    _singletonAsyncDisposables.Add(asyncDisposable);
                    break;
                case IDisposable disposable:
                    _singletonDisposables.Add(disposable);
                    break;
            }
        }
    }

    internal void UntrackSingletonDisposable(object instance)
    {
        lock (_registrationLock)
        {
            if (instance is IAsyncDisposable asyncDisposable)
            {
                _singletonAsyncDisposables.Remove(asyncDisposable);
            }

            if (instance is IDisposable disposable)
            {
                _singletonDisposables.Remove(disposable);
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }
    }

    private readonly struct ResolutionResult
    {
        public ResolutionResult(object instance, string topicPattern, IReadOnlyDictionary<string, object> capturedValues, string? key)
        {
            Instance = instance;
            TopicPattern = topicPattern;
            CapturedValues = capturedValues;
            Key = key;
        }

        public object Instance { get; }
        public string TopicPattern { get; }
        public IReadOnlyDictionary<string, object> CapturedValues { get; }
        public string? Key { get; }
    }

    public sealed class TopicServiceResolutionContext
    {
        internal TopicServiceResolutionContext(IServiceProvider services, string topic, string topicPattern, IReadOnlyDictionary<string, object> capturedValues)
        {
            Services = services;
            Topic = topic;
            TopicPattern = topicPattern;
            CapturedValues = capturedValues;
        }

        public IServiceProvider Services { get; }
        public string Topic { get; }
        public string TopicPattern { get; }
        public IReadOnlyDictionary<string, object> CapturedValues { get; }
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
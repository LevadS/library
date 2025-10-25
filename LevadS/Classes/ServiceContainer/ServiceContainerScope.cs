using LevadS.Classes;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS;

internal sealed class ServiceContainerScope : IServiceResolver
{
    private readonly ServiceContainer _container;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceScope? _innerScope;
    private readonly bool _ownsScope;
    private readonly Dictionary<ServiceRegistration, object> _scopedInstances = new();
    private readonly List<IAsyncDisposable> _asyncDisposables = new();
    private readonly List<IDisposable> _disposables = new();
    private readonly object _scopedInstancesLock = new();
    private readonly object _transientLock = new();
    private bool _disposed;
    private readonly object _scopeLock = new();
    private readonly HashSet<ServiceContainerScope> _scopes = new();
    private ServiceContainerScope? _parentScope = null;

    internal ServiceContainerScope(ServiceContainer container, IServiceProvider serviceProvider, IServiceScope? innerScope, bool ownsScope, ServiceContainerScope? parentScope = null)
    {
        _container = container;
        _serviceProvider = serviceProvider;
        _scopeFactory = _serviceProvider.GetService<IServiceScopeFactory>()
                        ?? throw new InvalidOperationException("The provided IServiceProvider does not expose an IServiceScopeFactory.");
        _innerScope = innerScope;
        _ownsScope = ownsScope;
        _parentScope = parentScope;
    }

    internal bool IsRoot => _parentScope == null;

    internal void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainerScope));
        }
    }

    internal object Resolve<TRequestedInput, TRequestedOutput>(ServiceRegistration registration, IContext context)
    {
        EnsureNotDisposed();

        if (registration.Lifetime == ServiceLifetime.Scoped && IsRoot)
        {
            throw new InvalidOperationException("Scoped services require an explicit topic scope. Call CreateScope() before resolving scoped registrations.");
        }

        return registration.Lifetime switch
        {
            ServiceLifetime.Singleton => registration.GetOrCreateSingleton<TRequestedInput, TRequestedOutput>(_container, context),
            ServiceLifetime.Scoped => GetScopedInstance<TRequestedInput, TRequestedOutput>(registration, context),
            _ => CreateTransientInstance<TRequestedInput, TRequestedOutput>(registration, context)
        };
    }

    private object GetScopedInstance<TRequestedInput, TRequestedOutput>(ServiceRegistration registration, IContext context)
    {
        lock (_scopedInstancesLock)
        {
            if (_scopedInstances.TryGetValue(registration, out var existing))
            {
                return existing;
            }

            ((Context)context).ServiceProvider = _serviceProvider;
            var instance = registration.CreateInstance<TRequestedInput, TRequestedOutput>(context);
            _scopedInstances.Add(registration, instance);
            return instance;
        }
    }

    private object CreateTransientInstance<TRequestedInput, TRequestedOutput>(ServiceRegistration registration, IContext context)
    {
        ((Context)context).ServiceProvider = _serviceProvider;
        var instance = registration.CreateInstance<TRequestedInput, TRequestedOutput>(context);
        TrackDisposable(instance);
        return instance;
    }

    private void TrackDisposable(object instance)
    {
        lock (_transientLock)
        {
            switch (instance)
            {
                case IAsyncDisposable asyncDisposable:
                    _asyncDisposables.Add(asyncDisposable);
                    break;
                case IDisposable disposable:
                    _disposables.Add(disposable);
                    break;
            }
        }
    }

    internal void RegisterScope(ServiceContainerScope containerScope)
    {
        if (containerScope.IsRoot) return;
        lock (_scopeLock)
        {
            _scopes.Add(containerScope);
        }
    }

    internal void UnregisterScope(ServiceContainerScope containerScope)
    {
        if (containerScope.IsRoot) return;
        lock (_scopeLock)
        {
            _scopes.Remove(containerScope);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        ServiceContainerScope[] scopesToDispose;
        lock (_scopeLock)
        {
            scopesToDispose = _scopes.ToArray();
            _scopes.Clear();
        }

        foreach (var scope in scopesToDispose)
        {
            await scope.DisposeAsync().ConfigureAwait(false);
        }

        _parentScope?.UnregisterScope(this);

        Dictionary<ServiceRegistration, object> scopedSnapshot;
        lock (_scopedInstancesLock)
        {
            scopedSnapshot = new Dictionary<ServiceRegistration, object>(_scopedInstances);
            _scopedInstances.Clear();
        }

        foreach (var instance in scopedSnapshot.Values)
        {
            switch (instance)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
        List<IAsyncDisposable> asyncDisposables;
        List<IDisposable> disposables;
        lock (_transientLock)
        {
            asyncDisposables = _asyncDisposables.ToList();
            disposables = _disposables.ToList();
            _asyncDisposables.Clear();
            _disposables.Clear();
        }

        foreach (var asyncDisposable in asyncDisposables)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }

        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }

        if (_ownsScope)
        {
            if (_innerScope is IAsyncDisposable asyncScope)
            {
                await asyncScope.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _innerScope?.Dispose();
            }
        }
    }

    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();

    internal void ReleaseRegistration(ServiceRegistration registration)
    {
        if (_disposed)
        {
            return;
        }

        ServiceContainerScope[] scopesSnapshot;
        lock (_scopeLock)
        {
            scopesSnapshot = _scopes.ToArray();
        }
        foreach (var scope in scopesSnapshot)
        {
            scope.ReleaseRegistration(registration);
        }

        object? instance = null;
        lock (_scopedInstancesLock)
        {
            if (_scopedInstances.TryGetValue(registration, out instance))
            {
                _scopedInstances.Remove(registration);
            }
        }

        if (instance == null)
        {
            return;
        }

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

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }
    }

    public IServiceProvider ServiceProvider => _serviceProvider;

    public IServiceResolver CreateScope()
    {
        ThrowIfDisposed();
        var serviceScope = _scopeFactory.CreateScope();
        var scope = new ServiceContainerScope(_container, serviceScope.ServiceProvider, serviceScope, ownsScope: true, this);
        RegisterScope(scope);
        return scope;
    }

    public IEnumerable<(TService, IContext)> GetServices<TService, TRequestedInput, TRequestedOutput>(IContext context,  string[]? keys = null)
        => _container.GetServices<TService, TRequestedInput, TRequestedOutput>(this, context, keys);

    public IEnumerable<(object, IContext)> GetServices<TRequestedInput, TRequestedOutput>(Type serviceType, IContext context,  string[]? keys = null)
        => _container.GetServices<TRequestedInput, TRequestedOutput>(serviceType, this, context, keys);
}
using System;
using System.Linq;
using System.Threading.Tasks;
using LevadS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LevadS.Tests.Tests;

// [TestClass]
// public sealed class TopicServiceContainerTests
// {
//     [TestMethod]
//     public void ResolvesMatchingServices()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<IMarker, AlphaService>("orders:*");
//
//             var services = container.GetServices<IMarker>("orders:created").ToArray();
//
//             Assert.AreEqual(1, services.Length);
//             Assert.IsInstanceOfType(services[0], typeof(AlphaService));
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void SkipsNonMatchingServices()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<IMarker, AlphaService>("orders:*");
//
//             var services = container.GetServices<IMarker>("users:created").ToArray();
//
//             Assert.AreEqual(0, services.Length);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void CapturedValuesAreReturned()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<CapturedService>("orders:{id:int}", ctx => new CapturedService((int)ctx.CapturedValues["id"]));
//
//             var resolution = container.GetServiceResolutions<CapturedService>("orders:42").Single();
//
//             Assert.AreEqual(42, resolution.Service.Value);
//             Assert.AreEqual(42, resolution.CapturedValues["id"]);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void SingletonIsSharedAcrossScopes()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<IMarker, AlphaService>("orders:*", ServiceLifetime.Singleton);
//
//             var rootInstance = container.GetService<IMarker>("orders:a");
//             using var scope = container.CreateScope();
//             var scopedInstance = container.GetService<IMarker>(scope, "orders:b");
//
//             Assert.IsNotNull(rootInstance);
//             Assert.AreSame(rootInstance, scopedInstance);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void ScopedRequiresExplicitScope()
//     {
//         var (container, provider) = CreateContainer(services => services.AddScoped<ScopedDependency>());
//         try
//         {
//             using var registration = container.Register<ScopedService>("orders:*", ctx => new ScopedService(ctx.Services.GetRequiredService<ScopedDependency>()), ServiceLifetime.Scoped);
//
//             Assert.ThrowsException<InvalidOperationException>(() =>
//             {
//                 return container.GetService<ScopedService>("orders:x");
//             });
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void ScopedIsReusedWithinScopeAndUniqueAcrossScopes()
//     {
//         var (container, provider) = CreateContainer(services => services.AddScoped<ScopedDependency>());
//         try
//         {
//             using var registration = container.Register<ScopedService>("orders:*", ctx => new ScopedService(ctx.Services.GetRequiredService<ScopedDependency>()), ServiceLifetime.Scoped);
//
//             using var scopeA = container.CreateScope();
//             var scopedA1 = container.GetService<ScopedService>(scopeA, "orders:foo");
//             var scopedA2 = container.GetService<ScopedService>(scopeA, "orders:bar");
//
//             using var scopeB = container.CreateScope();
//             var scopedB = container.GetService<ScopedService>(scopeB, "orders:baz");
//
//             Assert.IsNotNull(scopedA1);
//             Assert.AreSame(scopedA1, scopedA2);
//             Assert.AreNotSame(scopedA1, scopedB);
//             Assert.AreSame(scopedA1!.Dependency, scopedA2!.Dependency);
//             Assert.AreNotSame(scopedA1.Dependency, scopedB!.Dependency);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void TransientCreatesNewInstanceEachTime()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<IMarker, AlphaService>("orders:*");
//
//             var first = container.GetService<IMarker>("orders:foo");
//             var second = container.GetService<IMarker>("orders:bar");
//
//             Assert.IsNotNull(first);
//             Assert.IsNotNull(second);
//             Assert.AreNotSame(first, second);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public async Task TransientIsDisposedWithScopeAsync()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<TrackableDisposable>("orders:*", _ => new TrackableDisposable());
//
//             var scope = container.CreateScope();
//             var instance = container.GetService<TrackableDisposable>(scope, "orders:a");
//             Assert.IsNotNull(instance);
//
//             await scope.DisposeAsync();
//
//             Assert.IsTrue(instance!.IsDisposed);
//         }
//         finally
//         {
//             await container.DisposeAsync();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public async Task ScopedIsDisposedWithScopeAsync()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             using var registration = container.Register<TrackableDisposable>("orders:*", _ => new TrackableDisposable(), ServiceLifetime.Scoped);
//
//             var scope = container.CreateScope();
//             var instance = container.GetService<TrackableDisposable>(scope, "orders:topic");
//             Assert.IsNotNull(instance);
//
//             await scope.DisposeAsync();
//
//             Assert.IsTrue(instance!.IsDisposed);
//         }
//         finally
//         {
//             await container.DisposeAsync();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public async Task SingletonAsyncDisposableIsDisposedOnContainerAsync()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             var registration = container.Register<TrackableAsyncDisposable>("orders:*", _ => new TrackableAsyncDisposable(), ServiceLifetime.Singleton);
//             try
//             {
//                 var instance = container.GetService<TrackableAsyncDisposable>("orders:item");
//                 Assert.IsNotNull(instance);
//
//                 await container.DisposeAsync();
//
//                 Assert.IsTrue(instance!.IsDisposed);
//             }
//             finally
//             {
//                 registration.Dispose();
//             }
//         }
//         finally
//         {
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void FactoryReceivesServiceProvider()
//     {
//         var (container, provider) = CreateContainer(services => services.AddSingleton<Dependency>());
//         try
//         {
//             using var registration = container.Register<DependentService>("orders:*", ctx => new DependentService(ctx.Services.GetRequiredService<Dependency>()));
//
//             var instance = container.GetService<DependentService>("orders:abc");
//
//             Assert.IsNotNull(instance);
//             Assert.IsNotNull(instance!.Dependency);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void InvalidPatternThrows()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             Assert.ThrowsException<ArgumentException>(() => container.Register<IMarker, AlphaService>(string.Empty));
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void NullTopicThrows()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             Assert.ThrowsException<ArgumentNullException>(() => container.GetServices<IMarker>((string)null!));
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void RegisterInstanceReturnsSameInstance()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             var instance = new AlphaService();
//             using var registration = container.RegisterInstance<AlphaService>("orders:*", instance);
//
//             var resolvedRoot = container.GetService<AlphaService>("orders:foo");
//             using var scope = container.CreateScope();
//             var resolvedScoped = container.GetService<AlphaService>(scope, "orders:bar");
//
//             Assert.AreSame(instance, resolvedRoot);
//             Assert.AreSame(instance, resolvedScoped);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void RegisterInstanceDisposesOnContainerDispose()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             var instance = new TrackableDisposable();
//             var registration = container.RegisterInstance<TrackableDisposable>("orders:*", instance);
//
//             container.Dispose();
//
//             Assert.IsTrue(instance.IsDisposed);
//
//             registration.Dispose();
//         }
//         finally
//         {
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void DisposingHandleRemovesRegistration()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             var registration = container.Register<IMarker, AlphaService>("orders:*");
//
//             var initial = container.GetService<IMarker>("orders:a");
//             Assert.IsNotNull(initial);
//
//             registration.Dispose();
//
//             var after = container.GetService<IMarker>("orders:a");
//             Assert.IsNull(after);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void DisposingHandleDisposesSingleton()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             var registration = container.Register<TrackableDisposable>("orders:*", _ => new TrackableDisposable(), ServiceLifetime.Singleton);
//             var instance = container.GetService<TrackableDisposable>("orders:x");
//             Assert.IsNotNull(instance);
//             Assert.IsFalse(instance!.IsDisposed);
//
//             registration.Dispose();
//
//             Assert.IsTrue(instance.IsDisposed);
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     [TestMethod]
//     public void DisposingHandleDisposesScopedInstances()
//     {
//         var (container, provider) = CreateContainer();
//         try
//         {
//             var registration = container.Register<TrackableDisposable>("orders:*", _ => new TrackableDisposable(), ServiceLifetime.Scoped);
//
//             using var scope = container.CreateScope();
//             var instance = scope.GetService<TrackableDisposable>("orders:topic");
//             Assert.IsNotNull(instance);
//             Assert.IsFalse(instance!.IsDisposed);
//
//             registration.Dispose();
//
//             Assert.IsTrue(instance.IsDisposed);
//             Assert.IsNull(scope.GetService<TrackableDisposable>("orders:again"));
//         }
//         finally
//         {
//             container.Dispose();
//             provider.Dispose();
//         }
//     }
//
//     private static (TopicServiceContainer Container, ServiceProvider Provider) CreateContainer(Action<IServiceCollection>? configure = null)
//     {
//         var services = new ServiceCollection();
//         configure?.Invoke(services);
//         var provider = services.BuildServiceProvider();
//         var container = new TopicServiceContainer()
//         {
//             RootServiceProvider = provider
//         };
//         return (container, provider);
//     }
//
//     private interface IMarker { }
//
//     private sealed class AlphaService : IMarker { }
//
//     private sealed class CapturedService
//     {
//         public CapturedService(int value) => Value = value;
//         public int Value { get; }
//     }
//
//     private sealed class ScopedDependency { }
//
//     private sealed class ScopedService
//     {
//         public ScopedService(ScopedDependency dependency) => Dependency = dependency;
//         public ScopedDependency Dependency { get; }
//     }
//
//     private sealed class Dependency { }
//
//     private sealed class DependentService
//     {
//         public DependentService(Dependency dependency) => Dependency = dependency;
//         public Dependency Dependency { get; }
//     }
//
//     private sealed class TrackableDisposable : IDisposable
//     {
//         public bool IsDisposed { get; private set; }
//         public void Dispose() => IsDisposed = true;
//     }
//
//     private sealed class TrackableAsyncDisposable : IAsyncDisposable
//     {
//         public bool IsDisposed { get; private set; }
//         public ValueTask DisposeAsync()
//         {
//             IsDisposed = true;
//             return ValueTask.CompletedTask;
//         }
//     }
// }

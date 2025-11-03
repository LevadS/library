using System.Reflection;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using LevadS.Extensions;
using LevadS.Interfaces;

namespace LevadS.Classes.Extensions;

internal static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates instance of given type. Constructor parameters are resolved from IServiceProvider,
    /// except parameters decorated with [FromTopic] are taken from topicValues dictionary (key is attribute.Name or parameter name).
    /// Additionally, for parameters without [FromTopic], the method will try to bind a topic value by parameter name
    /// (case-insensitive) and by type when provider cannot supply the service.
    /// Parameters with default values are honored when neither provider nor topic supplies a value.
    /// Behavior is similar to ActivatorUtilities.CreateInstance but supports topic-supplied values.
    /// </summary>
    internal static object CreateInstanceWithTopic(this IServiceProvider provider, Type type, IContext context)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(context);
    
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        ConstructorInfo? bestConstructor = null;
        object[]? bestArgs = null;
        var bestParamCount = -1;
    
        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            var ok = !parameters
                .Where((parameterInfo, i) =>
                    !parameterInfo.HandleParametersFromTopic(args, i, context.CapturedValues) &&
                    !parameterInfo.HandleMessageContextParameter(args, i, context) &&
                    !parameterInfo.HandleParametersFromServiceProvider(args, i, provider) &&
                    !parameterInfo.HandleImplicitParametersFromTopic(args, i, context.CapturedValues) &&
                    !parameterInfo.HandleParameterDefaultValue(args, i))
                .Any();
    
            if (!ok) continue;
    
            // prefer ctor with more parameters (like ActivatorUtilities)
            if (parameters.Length > bestParamCount)
            {
                bestParamCount = parameters.Length;
                bestConstructor = ctor;
                bestArgs = args!;
            }
        }
    
        if (bestConstructor == null)
        {
            throw new InvalidOperationException($"No suitable public constructor found for type {type.FullName} that can be satisfied by the service provider and topic values.");
        }
    
        return bestConstructor.Invoke(bestArgs);
    }

    internal static T CreateInstanceWithTopic<T>(this IServiceProvider provider, IContext context)
        => (T)CreateInstanceWithTopic(provider, typeof(T), context);
}
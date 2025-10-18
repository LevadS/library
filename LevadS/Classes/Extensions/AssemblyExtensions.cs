using System.Reflection;
using LevadS.Attributes;

internal static class AssemblyExtensions
{
    /// <summary>
    /// Finds types in the assembly that implement the given interface and are decorated with the given attribute.
    /// </summary>
    public static IEnumerable<Type> FindImplementingTypes(this Assembly assembly, Type interfaceType)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(interfaceType);
        if (!interfaceType.IsInterface) throw new ArgumentException("interfaceType must be an interface", nameof(interfaceType));

        foreach (var ti in assembly.DefinedTypes)
        {
            var type = ti.AsType();

            if (!ti.IsClass) continue;

            if (!ImplementsInterface(ti, interfaceType)) continue;

            if (ti.GetCustomAttributes<BaseLevadSRegistrationAttribute>(true).Any())
            {
                yield return type;
            }
        }
    }

    private static bool ImplementsInterface(TypeInfo typeInfo, Type interfaceType)
        => interfaceType.IsGenericTypeDefinition
            ? typeInfo.ImplementedInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
            : interfaceType.IsAssignableFrom(typeInfo.AsType());
}
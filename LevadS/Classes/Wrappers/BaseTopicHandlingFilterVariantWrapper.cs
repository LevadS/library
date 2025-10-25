using System.Reflection;
using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class BaseTopicFilterVariantWrapper(object filter) : IKeyedHandler
{
    // Await a Task or Task<T> and return the result cast to the requested type (as object).
    protected static async Task<object?> AwaitAndCastAsync<TService>(Task task)
    {
        await task.ConfigureAwait(false);

        var taskType = task.GetType();
        if (!taskType.IsGenericType)
            return null; // Task (non-generic) has no result

        // get Task<T>.Result via reflection
        var result = taskType.GetProperty("Result", BindingFlags.Instance | BindingFlags.Public)!.GetValue(task);

        return ConvertResultToTarget<TService>(result);
    }

    protected static object? ConvertResultToTarget<TService>(object? result)
    {
        var targetType = typeof(TService);
        if (result is null) return null;
        if (targetType.IsInstanceOfType(result)) return result;

        // try simple conversions for primitives / IConvertible
        if (result is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
            return Convert.ChangeType(result, targetType);

        // enums
        if (targetType.IsEnum)
            return Enum.ToObject(targetType, result);

        // last resort — if targetType is reference and result can be assigned
        if (targetType.IsAssignableFrom(result.GetType())) return result;

        throw new InvalidCastException($"Cannot convert result of type {result.GetType()} to {targetType}");
    }

    public string TopicPattern => ((ITopicHandler)filter).TopicPattern;
    public string Key => ((ITopicHandler)filter).Key;
    public Context? Context
    {
        get => ((ITopicHandler)filter).Context;
        set => ((ITopicHandler)filter).Context = value;
    }
}
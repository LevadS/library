using LevadS.Interfaces;

namespace LevadS.Attributes;

public abstract class BaseLevadSFilterForAttribute : Attribute
{
    public abstract Type FilteredHandlerType { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LevadSFilterForAttribute<TFilteredHandler> : BaseLevadSFilterForAttribute
    where TFilteredHandler : IHandler
{
    public override Type FilteredHandlerType => typeof(TFilteredHandler);
}
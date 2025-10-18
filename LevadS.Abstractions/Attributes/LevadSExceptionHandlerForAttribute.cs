using LevadS.Interfaces;

namespace LevadS.Attributes;

public abstract class BaseLevadSExceptionHandlerForAttribute : Attribute
{
    public abstract Type ProtectedHandlerType { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LevadSExceptionHandlerForAttribute<TProtectedHandler> : BaseLevadSExceptionHandlerForAttribute
    where TProtectedHandler : IHandler
{
    public override Type ProtectedHandlerType => typeof(TProtectedHandler);
}
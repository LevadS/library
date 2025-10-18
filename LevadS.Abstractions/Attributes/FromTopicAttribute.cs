namespace LevadS.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromTopicAttribute(string? name = null) : Attribute
{
    public string? Name => name;
}
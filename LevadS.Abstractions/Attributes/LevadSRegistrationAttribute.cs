namespace LevadS.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class LevadSRegistrationAttribute(string topicPattern = "*")
    : BaseLevadSRegistrationAttribute(topicPattern);

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LevadSRegistrationAttribute<TInterface>(string topicPattern = "*")
    : LevadSRegistrationAttribute(topicPattern)
{
    public override Type InterfaceType => typeof(TInterface);
}
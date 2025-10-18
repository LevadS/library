namespace LevadS.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public abstract class BaseLevadSGenericRegistrationAttribute(string topicPattern = "*")
    : BaseLevadSRegistrationAttribute(topicPattern);

/// <summary>
/// Marks generic handler/filter class to be registered in LevadS; all handler/filter interfaces
/// implemented by class will be used in registration
/// </summary>
/// <param name="topicPattern">Topic pattern</param>
/// <typeparam name="TService">Closed generic type to be registered</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class LevadSGenericRegistrationAttribute<TService>(string topicPattern = "*")
    : BaseLevadSGenericRegistrationAttribute(topicPattern)
{
    public override Type ServiceType => typeof(TService);
}

/// <summary>
/// Marks generic handler/filter class to be registered in LevadS; only interface that is
/// specified in this attribute and implemented by class will be used in registration
/// </summary>
/// <param name="topicPattern">Topic pattern</param>
/// <typeparam name="TInterface">Interface that will be used in registration</typeparam>
/// <typeparam name="TService">Closed generic type to be registered</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class LevadSGenericRegistrationAttribute<TInterface, TService>(string topicPattern = "*")
    : LevadSGenericRegistrationAttribute<TService>(topicPattern)
{
    public override Type InterfaceType => typeof(TInterface);
}
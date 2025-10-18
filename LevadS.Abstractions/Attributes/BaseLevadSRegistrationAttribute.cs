using LevadS.Extensions;

namespace LevadS.Attributes;

public abstract class BaseLevadSRegistrationAttribute : Attribute
{
    protected BaseLevadSRegistrationAttribute(string topicPattern)
    {
        if (!string.IsNullOrEmpty(topicPattern) && !topicPattern.IsValidTopicPattern(out var errorMessage))
        {
            throw new ArgumentException(errorMessage, nameof(topicPattern));
        }
        
        TopicPattern = topicPattern;
    }
    public string TopicPattern { get; private set; }
    public virtual Type? InterfaceType => null;
    public virtual Type? ServiceType => null;
}
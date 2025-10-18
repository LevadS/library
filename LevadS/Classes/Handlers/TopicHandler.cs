using LevadS.Interfaces;

namespace LevadS.Classes;

internal abstract class TopicHandler(
    IServiceProvider serviceProvider,
    Func<TopicHandler, object> serviceFactory,
    string topicPattern,
    string key
) : ITopicHandler
{
    public string TopicPattern => topicPattern;
    
    public string Key => key;

    protected object ServiceObject => serviceFactory(this);
    
    public Context? Context { get; set; }
    
    public IServiceProvider ServiceProvider => serviceProvider;
}
using LevadS.Enums;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal abstract class Context(IServiceProvider serviceProvider) : IContext
{
    public object? MessageObject { get; protected init; }
    
    public Type MessageType => MessageObject?.GetType() ?? throw new NullReferenceException();
    
    internal Dictionary<string, object> Headers = new ();

    IReadOnlyDictionary<string, object> IContext.Headers => Headers;
    
    public string? Topic { get; internal set; }
    
    string IContext.Topic => Topic!;
    
    internal Dictionary<string, object> CapturedTopicValues { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    IReadOnlyDictionary<string, object> IContext.CapturedTopicValues => CapturedTopicValues;
    
    public DispatchType DispatchType { get; init; }
    
    public CancellationToken CancellationToken { get; init; }
    
    public IServiceProvider ServiceProvider { get; init; } = serviceProvider;
}

internal class MessageContext<TMessage> : Context, IMessageContext<TMessage>
{
    internal MessageContext(IServiceProvider serviceProvider) : base(serviceProvider) {}
    
    internal MessageContext(Context context) : base(context.ServiceProvider)
    {
        MessageObject = context.MessageObject;
        Headers = context.Headers.ToDictionary();
        Topic = context.Topic;
        foreach (var topicValue in context.CapturedTopicValues)
        {
            CapturedTopicValues.Add(topicValue.Key, topicValue.Value);
        }
        DispatchType = context.DispatchType;
        CancellationToken = context.CancellationToken;
    }
    
    public TMessage Message
    {
        get => (TMessage)MessageObject!;
        internal init => MessageObject = value!;
    }

    public virtual Context Clone()
        => new MessageContext<TMessage>(this)
        {
            MessageObject = MessageObject,
            Topic = Topic
        };
}
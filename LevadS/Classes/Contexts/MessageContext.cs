using LevadS.Enums;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal abstract class Context(IServiceProvider serviceProvider) : IContext
{
    public object? MessageObject { get; protected init; }
    
    public Type MessageType => MessageObject?.GetType() ?? throw new NullReferenceException();
    
    internal Dictionary<string, object> Headers = new ();

    IReadOnlyDictionary<string, object> IContext.Headers => Headers;

    public string Topic { get; internal set; } = "";
    
    public string? Key { get; internal set; }
    
    string IContext.Topic => Topic!;
    
    public IReadOnlyDictionary<string, object> CapturedValues { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    
    public DispatchType DispatchType { get; set; }
    
    public CancellationToken CancellationToken { get; set; }
    
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;

    public abstract Context CloneInstance();
}

internal class MessageContext<TMessage> : Context, IMessageContext<TMessage>
{
    internal MessageContext(IServiceProvider serviceProvider) : base(serviceProvider) {}
    
    internal MessageContext(Context context) : base(context.ServiceProvider)
    {
        MessageObject = context.MessageObject;
        Headers = context.Headers.ToDictionary();
        Topic = context.Topic;
        CapturedValues = new Dictionary<string, object>(context.CapturedValues, StringComparer.OrdinalIgnoreCase);
        DispatchType = context.DispatchType;
        CancellationToken = context.CancellationToken;
    }
    
    public TMessage Message
    {
        get => (TMessage)MessageObject!;
        internal init => MessageObject = value!;
    }

    public override Context CloneInstance()
        => new MessageContext<TMessage>(this)
        {
            MessageObject = MessageObject,
            Topic = Topic
        };
}
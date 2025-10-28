using LevadS.Interfaces;

namespace LevadS.Classes;

internal class StreamContext<TRequest> : MessageContext<TRequest>, IStreamContext<TRequest>
{
    internal StreamContext(IServiceProvider serviceProvider) : base(serviceProvider) {}
    
    internal StreamContext(Context context) : base(context) {}
    
    public TRequest Request
    {
        get => Message;
        internal init => Message = value;
    }

    public override Context CloneInstance()
        => new StreamContext<TRequest>(this)
        {
            MessageObject = MessageObject,
            Topic = Topic
        };
}
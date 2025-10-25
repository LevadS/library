using LevadS.Interfaces;

namespace LevadS.Classes;

internal class RequestContext<TRequest> : MessageContext<TRequest>, IRequestContext<TRequest>
{
    internal RequestContext(IServiceProvider serviceProvider) : base(serviceProvider) {}
    
    internal RequestContext(Context context) : base(context) {}
    
    public TRequest Request
    {
        get => Message;
        internal init => Message = value;
    }

    public override Context CloneInstance()
        => new RequestContext<TRequest>(this)
        {
            MessageObject = MessageObject,
            Topic = Topic
        };
}
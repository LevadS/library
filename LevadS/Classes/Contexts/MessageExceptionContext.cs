using LevadS.Interfaces;

namespace LevadS.Classes;

internal class MessageExceptionContext<TMessage, TException> : MessageContext<TMessage>, IMessageExceptionContext<TMessage, TException>
    where TException : Exception
{
    public TException Exception { get; }

    internal MessageExceptionContext(MessageContext<TMessage> context, TException exception) : base(context)
    {
        Exception = exception;
    }

    public override Context CloneInstance()
        => new MessageExceptionContext<TMessage, TException>(this, Exception)
        {
            MessageObject = MessageObject,
            Topic = Topic
        };
}
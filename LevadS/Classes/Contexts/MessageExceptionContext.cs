using LevadS.Interfaces;

namespace LevadS.Classes;

internal class MessageExceptionContext<TMessage, TException> : RequestContext<TMessage>, IMessageExceptionContext<TMessage, TException>
    where TException : Exception
{
    public TException Exception { get; }

    internal MessageExceptionContext(MessageContext<TMessage> context, TException exception) : base(context)
    {
        Exception = exception;
    }
}
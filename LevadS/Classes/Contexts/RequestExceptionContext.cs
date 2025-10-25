using LevadS.Interfaces;

namespace LevadS.Classes;

internal class RequestExceptionContext<TRequest, TException> : RequestContext<TRequest>, IRequestExceptionContext<TRequest, TException>
    where TException : Exception
{
    public TException Exception { get; }
    
    internal RequestExceptionContext(RequestContext<TRequest> context, TException exception) : base(context)
    {
        Exception = exception;
    }

    public override Context CloneInstance()
        => new RequestExceptionContext<TRequest, TException>(this, Exception)
        {
            MessageObject = MessageObject,
            Topic = Topic
        };
}
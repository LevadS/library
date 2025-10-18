using LevadS.Interfaces;

namespace LevadS.Classes;

internal class StreamExceptionContext<TRequest, TException> : StreamContext<TRequest>, IStreamExceptionContext<TRequest, TException>
    where TException : Exception
{
    public TException Exception { get; internal init; }
    
    internal StreamExceptionContext(StreamContext<TRequest> context, TException exception) : base(context)
    {
        Exception = exception;
    }
}
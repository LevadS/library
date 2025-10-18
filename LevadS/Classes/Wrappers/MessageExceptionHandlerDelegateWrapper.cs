using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

internal class MessageExceptionHandlerDelegateWrapper<TMessage, TException>(MessageExceptionHandlerDelegate<TMessage, TException> exceptionHandler) : IMessageHandlingFilter<TMessage>, IExceptionHandler
    where TException : Exception
{
    public async Task InvokeAsync(IMessageContext<TMessage> messageContext, MessageHandlingFilterNextDelegate next)
    {
        try
        {
            await next();
        }
        catch (TException e)
        {
            var handled =
                await exceptionHandler.Invoke(new MessageExceptionContext<TMessage, TException>((MessageContext<TMessage>)messageContext, e));

            if (!handled)
            {
                throw;
            }
        }
    }
}
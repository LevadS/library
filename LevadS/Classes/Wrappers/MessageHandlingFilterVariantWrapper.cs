using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

#pragma warning disable CS9107 // captured into the state and also passed to base

internal class MessageFilterVariantWrapper<TMessage>(object filter)
    : BaseTopicFilterVariantWrapper(filter), IMessageHandlingFilter<TMessage>
{
    public Task InvokeAsync(IMessageContext<TMessage> context,
        MessageHandlingFilterNextDelegate next)
    {
        if (context is not Context baseContext)
            throw new InvalidOperationException("Context must derive from Context");
        var messageContext = new MessageContext<object>(baseContext);
        var method = filter.GetType().GetMethods().First(m => m.Name == "InvokeAsync");
        var invoked = method.Invoke(filter, [messageContext, (MessageHandlingFilterNextDelegate)(async () => await next())]);
        if (invoked is not Task task)
        {
            throw new InvalidOperationException("InvokeAsync must return a Task");
        }
        return task;
    }
}

#pragma warning restore CS9107
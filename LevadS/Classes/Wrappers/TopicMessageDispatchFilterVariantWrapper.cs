using LevadS.Delegates;
using LevadS.Interfaces;

namespace LevadS.Classes;

#pragma warning disable CS9107

internal class TopicMessageDispatchFilterVariantWrapper<TMessage>(object filter)
    : BaseTopicFilterVariantWrapper(filter), ITopicMessageDispatchFilter<TMessage>
{
    public Task InvokeAsync(IMessageContext<TMessage> context,
        MessageDispatchFilterNextDelegate next)
    {
        var messageContext = new MessageContext<object>((Context)context);
        var method = filter.GetType().GetMethods().First(m => m.Name == "InvokeAsync");
        var invoked = method.Invoke(filter, [messageContext, (MessageDispatchFilterNextDelegate)(async (t, h) => await next(t, h))]);
        if (invoked is not Task task)
        {
            throw new InvalidOperationException("InvokeAsync must return a Task");
        }
        return task;
    }
}

#pragma warning restore CS9107
using System.Reflection;
using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Handlers;

[
    LevadSGenericRegistration<GenericMessageHandler<int>>,
    LevadSGenericRegistration<GenericMessageHandler<string>>
]
public class GenericMessageHandler<T> : IMessageHandler<GenericMessage<T>>
{
    public static bool Executed = false;

    public Task HandleAsync(IMessageContext<GenericMessage<T>> requestContext)
    {
        Executed = true;
        
        return Task.CompletedTask;
    }
}
using System.Diagnostics;
using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Examples.Messages;

namespace LevadS.Examples.Handlers;

[LevadSGenericRegistration<GenericHandler<bool>>]
[LevadSGenericRegistration<GenericHandler<object>>]
[LevadSGenericRegistration<GenericHandler<WeatherForecastRequest>>]
class GenericHandler<T> : IMessageHandler<T>
{
    public Task HandleAsync(IMessageContext<T> messageContext)
    {
        Debug.WriteLine($"{GetType().FullName}: {messageContext.GetType().FullName}");
        
        return Task.CompletedTask;
    }
}

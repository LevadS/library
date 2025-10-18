using System.Diagnostics;
using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Examples.Handlers;

namespace LevadS.Examples.Filters;

[LevadSRegistration]
[LevadSFilterFor<StringHandler>]
public class StringFilter : IMessageHandlingFilter<string>
{
    public async Task InvokeAsync(IMessageContext<string> messageContext, MessageHandlingFilterNextDelegate next)
    {
        Debug.WriteLine("StringFilter: before");
        await next();
        Debug.WriteLine("StringFilter: after");
    }
}
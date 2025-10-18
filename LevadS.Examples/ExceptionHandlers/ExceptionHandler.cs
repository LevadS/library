using LevadS.Attributes;
using LevadS.Interfaces;

namespace LevadS.Examples.ExceptionHandlers;

[LevadSRegistration]
public class ExceptionHandler : IMessageExceptionHandler<int, Exception>
{
    public Task<bool> HandleAsync(IMessageExceptionContext<int, Exception> exceptionContext)
    {
        return Task.FromResult(true); // handled
    }
}
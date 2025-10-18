using LevadS.Attributes;
using LevadS.Delegates;
using LevadS.Examples.Exceptions;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.ExceptionHandlers;

[LevadSRegistration]
public class StreamExceptionHandler : IStreamExceptionHandler<IntStreamRequest, int, IntException>
{
    public Task<bool> HandleAsync(IStreamExceptionContext<IntStreamRequest, IntException> exceptionContext, StreamExceptionHandlerFallbackDelegate<int> fallbackCallback)
    {
        fallbackCallback(exceptionContext.Exception.FallbackValue);
        fallbackCallback(exceptionContext.Exception.FallbackValue + 1);
        fallbackCallback(exceptionContext.Exception.FallbackValue + 2);

        return Task.FromResult(true);
    }
}
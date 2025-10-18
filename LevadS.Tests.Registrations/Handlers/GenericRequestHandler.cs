using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Tests.Registrations.Messages;

namespace LevadS.Tests.Handlers;

[
    LevadSGenericRegistration<GenericRequestHandler<int>>,
    LevadSGenericRegistration<GenericRequestHandler<string>>
]
public class GenericRequestHandler<T> : IRequestHandler<GenericRequest<T>, T>
{
    public Task<T> HandleAsync(IRequestContext<GenericRequest<T>> requestContext)
    {
        return Task.FromResult(default(T)!);
    }
}
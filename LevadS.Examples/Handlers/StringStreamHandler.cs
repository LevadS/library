using System.Runtime.CompilerServices;
using LevadS.Attributes;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
public class StringStreamHandler : IStreamHandler<StringStreamRequest, string>
{
    public async IAsyncEnumerable<string> HandleAsync(IStreamContext<StringStreamRequest> requestContext, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var i in Enumerable.Range(1, 10))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return $"Message #{i}";
            
            await Task.Delay(500, cancellationToken);
        }
    }
}
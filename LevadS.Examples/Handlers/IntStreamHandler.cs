using System.Runtime.CompilerServices;
using LevadS.Attributes;
using LevadS.Examples.Exceptions;
using LevadS.Examples.Messages;
using LevadS.Interfaces;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
public class IntStreamHandler : IStreamHandler<IntStreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(IStreamContext<IntStreamRequest> streamContext, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var range = Enumerable.Range(1, 100).ToArray();
        foreach (var value in range)
        {
            // if (value % 3 == 0)
            // {
            //     Debug.WriteLine($"falling back to: {value}");
            //     throw new IntException(value);
            // }
            
            yield return value;
            
            // await Task.Delay(100, cancellationToken);
        }
    }
}
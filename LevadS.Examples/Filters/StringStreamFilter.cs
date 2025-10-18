using System.Diagnostics;
using LevadS.Delegates;
using LevadS.Interfaces;
using LevadS.Examples.Messages;

namespace LevadS.Examples.Filters;

public class StringStreamFilter : IStreamHandlingFilter<StringStreamRequest, string>
{
    public IAsyncEnumerable<string> InvokeAsync(IStreamContext<StringStreamRequest> streamContext, StreamHandlingFilterNextDelegate<string> next)
    {
        Debug.WriteLine("Stream filter before");
        var result = next();
        Debug.WriteLine("Stream filter after");
        
        return result;
    }
}
namespace LevadS;

/// <summary>
/// Class used to envelop request messages that are not implementing <see cref="IRequest&lt;TResponse&gt;"/>.
/// </summary>
/// <param name="data">Enveloped request message</param>
/// <typeparam name="TRequest">Request message type</typeparam>
/// <typeparam name="TResponse">Response message type</typeparam>
public sealed class Request<TRequest, TResponse>(TRequest data) : IRequest<TResponse>
{
    public Request() : this(default!)
    { }

    public TRequest Data { get; set; } = data;
}
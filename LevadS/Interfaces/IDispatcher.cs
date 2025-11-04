namespace LevadS;

public interface IDispatcher
{
    /// <summary>
    /// Sends message to single message handler. Message will be handled by first matching handler.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="topic">Message topic; default value: "" (empty string)</param>
    /// <param name="headers">Message headers; default header set: empty (null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <returns>Message handling task. Await this task to continue after handling is finished or
    /// dismiss it if you want to continue immediately.</returns>
    Task SendAsync<TMessage>(TMessage message, string topic = "", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null);

    /// <summary>
    /// Publishes (broadcasts) message to multiple message handlers. Message will be handled by all matching handlers.
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="topic">Message topic; default value: "" (empty string)</param>
    /// <param name="headers">Message headers; default header set: empty (null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <returns>Message handling task. Await this task to continue after handling is finished or
    /// dismiss it if you want to continue immediately.</returns>
    Task PublishAsync<TMessage>(TMessage message, string topic = "", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null);

    /// <summary>
    /// Requests a response from single request handler using request message that implements
    /// <see cref="IRequest&lt;TResponse&gt;"/>. Request will be handled by first matching handler.
    /// </summary>
    /// <param name="request">Request message</param>
    /// <param name="topic">Request topic; default value: "" (empty string)</param>
    /// <param name="headers">Request headers; default header set: empty (null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Type of requested response message</typeparam>
    /// <returns>Request handling task. Await this task to continue after handling is finished and get a response or
    /// dismiss it if you want to continue immediately.</returns>
    Task<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, string topic = "", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Requests a response from single request handler. Request will be handled by first matching handler.
    /// </summary>
    /// <param name="request">Request message</param>
    /// <param name="topic">Request topic; default value: "" (empty string)</param>
    /// <param name="headers">Request headers; default header set: empty (null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Type of requested response message</typeparam>
    /// <returns>Request handling task. Await this task to continue after handling is finished and get a response or
    /// dismiss it if you want to continue immediately.</returns>
    Task<TResponse> RequestAsync<TResponse>(object request, string topic = "", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Requests a stream of responses from single stream handler using stream request message that implements
    /// <see cref="IRequest&lt;TResponse&gt;"/>. Request will be handled by first matching handler.
    /// </summary>
    /// <param name="request">Stream request message</param>
    /// <param name="topic">Stream request topic; default value: "" (empty string)</param>
    /// <param name="headers">Stream request headers; default header set: empty (null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Type of requested streamed response message</typeparam>
    /// <returns>Request handling task. Await this task to continue after handling is finished and get a stream of
    /// responses or dismiss it if you want to continue immediately.</returns>
    IAsyncEnumerable<TResponse> StreamAsync<TResponse>(IRequest<TResponse> request, string topic = "", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null);
    
    /// <summary>
    /// Requests a stream of responses from single stream handler. Request will be handled by first matching handler.
    /// </summary>
    /// <param name="request">Stream request message</param>
    /// <param name="topic">Stream request topic; default value: "" (empty string)</param>
    /// <param name="headers">Stream request headers; default header set: empty (null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResponse">Type of requested streamed response message</typeparam>
    /// <returns>Request handling task. Await this task to continue after handling is finished and get a stream of
    /// responses or dismiss it if you want to continue immediately.</returns>
    IAsyncEnumerable<TResponse> StreamAsync<TResponse>(object request, string topic = "", Dictionary<string, object>? headers = null, CancellationToken? cancellationToken = null);
}
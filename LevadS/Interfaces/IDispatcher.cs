namespace LevadS;

public interface IDispatcher
{
    /// <summary>
    /// Sends message to single <a href="https://github.com/LevadS/LevadS/wiki/Message-handlers">message handler</a>.<br/><br/>
    /// Message will be handled by first <a href="https://github.com/LevadS/LevadS/wiki/Handlers-matching">matching handler</a>, f.e.:
    /// <code >
    ///   // delegate-based handler
    ///   builder.AddMessageHandler&lt;MyMessage&gt;(/* ... */);
    ///   
    ///   // class-based handler
    ///   class MyMessageHandler : IMessageHandler&lt;MyMessage&gt;
    ///   {
    ///       /* ... */
    ///   }
    /// </code>
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
    /// Publishes (broadcasts) message to multiple <a href="https://github.com/LevadS/LevadS/wiki/Message-handlers">message handlers</a>.<br/><br/>
    /// Message will be handled by all <a href="https://github.com/LevadS/LevadS/wiki/Handlers-matching">matching handlers</a>, f.e.:
    /// <code >
    ///   // delegate-based handler
    ///   builder.AddMessageHandler&lt;MyMessage&gt;(/* ... */);
    ///   
    ///   // class-based handler
    ///   class MyMessageHandler : IMessageHandler&lt;MyMessage&gt;
    ///   {
    ///       /* ... */
    ///   }
    /// </code>
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
    /// Requests a response from single <a href="https://github.com/LevadS/LevadS/wiki/Request-handlers">request handler</a> using request message that implements
    /// <see cref="IRequest&lt;TResponse&gt;"/>.<br/><br/>
    /// Request will be handled by first <a href="https://github.com/LevadS/LevadS/wiki/Handlers-matching">matching handler</a>, f.e.:
    /// <code >
    ///   // delegate-based handler
    ///   builder.AddRequestHandler&lt;StringRequest, string&gt;(/* ... */);
    ///   
    ///   // class-based handler
    ///   class StringRequestHandler : IRequestHandler&lt;StringRequest, string&gt;
    ///   {
    ///       /* ... */
    ///   }
    /// </code>
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
    /// Requests a response from single <a href="https://github.com/LevadS/LevadS/wiki/Request-handlers">request handler</a>.<br/><br/>
    /// Request will be handled by first <a href="https://github.com/LevadS/LevadS/wiki/Handlers-matching">matching handler</a>, f.e.:
    /// <code >
    ///   // delegate-based handler
    ///   builder.AddRequestHandler&lt;StringRequest, string&gt;(
    ///       (StringRequest req) => { /* ... */ }
    ///   );
    ///   
    ///   // class-based handler
    ///   class StringRequestHandler : IRequestHandler&lt;StringRequest, string&gt;
    ///   {
    ///       public Task&lt;string&gt; HandleAsync(IRequestContext&lt;StringRequest&gt; ctx)
    ///       {
    ///           /* ... */
    ///       }
    ///   }
    /// </code>
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
    /// Requests a stream of responses from single <a href="https://github.com/LevadS/LevadS/wiki/Stream-handlers">stream handler</a>
    /// using stream request message that implements <see cref="IRequest&lt;TResponse&gt;"/>.<br/><br/>
    /// Request will be handled by first <a href="https://github.com/LevadS/LevadS/wiki/Handlers-matching">matching handler</a>, f.e.:
    /// <code >
    ///   // class-based handler
    ///   class StringStreamRequestHandler : IStreamHandler&lt;StringRequest, string&gt;
    ///   {
    ///       public IAsyncEnumerable&lt;string&gt; HandleAsync(IStreamContext&lt;StringRequest&gt; ctx)
    ///       {
    ///           /* ... */
    ///       }
    ///   }
    /// </code>
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
    /// Requests a stream of responses from single <a href="https://github.com/LevadS/LevadS/wiki/Stream-handlers">stream handler</a>.<br/><br/>
    /// Request will be handled by first <a href="https://github.com/LevadS/LevadS/wiki/Handlers-matching">matching handler</a>, f.e.:
    /// <code >
    ///   // class-based handler
    ///   class StringStreamRequestHandler : IStreamHandler&lt;StringRequest, string&gt;
    ///   {
    ///       public IAsyncEnumerable&lt;string&gt; HandleAsync(IStreamContext&lt;StringRequest&gt; ctx)
    ///       {
    ///           /* ... */
    ///       }
    ///   }
    /// </code>
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
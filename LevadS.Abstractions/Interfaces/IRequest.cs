namespace LevadS;

/// <summary>
/// Interface that simplifies request and stream calls.<br/><br/>
/// When request message implements IRequest&lt;TResponse&gt;, dispatcher call can be simplified by omitting TResponse
/// type parameter. 
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequest<out TResponse>;
namespace LevadS;

/// <summary>
/// Interface to be implemented by messages used as requests and stream requests.
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequest<out TResponse>;
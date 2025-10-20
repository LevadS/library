using LevadS.Classes;

namespace LevadS.Interfaces;

internal interface ITopicHandler
{
    string TopicPattern { get; }
    
    string Key { get; }
    
    Context? Context { get; internal set; }
}

internal interface ITopicMessageHandler<in TMessage> : ITopicHandler, IMessageHandler<TMessage>;

internal interface ITopicRequestHandler<in TRequest, TResponse> : ITopicHandler, IRequestHandler<TRequest, TResponse>;

internal interface ITopicStreamHandler<in TRequest, out TResponse> : ITopicHandler, IStreamHandler<TRequest, TResponse>;

internal interface ITopicMessageHandlingFilter<in TMessage> : ITopicHandler, IMessageHandlingFilter<TMessage>;

internal interface ITopicRequestHandlingFilter<in TRequest, TResponse> : ITopicHandler, IRequestHandlingFilter<TRequest, TResponse>;

internal interface ITopicStreamHandlingFilter<in TRequest, TResponse> : ITopicHandler, IStreamHandlingFilter<TRequest, TResponse>;

internal interface ITopicMessageDispatchFilter<in TMessage> : ITopicHandler, IMessageDispatchFilter<TMessage>;

internal interface ITopicRequestDispatchFilter<in TRequest, TResponse> : ITopicHandler, IRequestDispatchFilter<TRequest, TResponse>;

internal interface ITopicStreamDispatchFilter<in TRequest, TResponse> : ITopicHandler, IStreamDispatchFilter<TRequest, TResponse>;
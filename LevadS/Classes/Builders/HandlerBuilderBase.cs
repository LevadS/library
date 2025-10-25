using LevadS.Interfaces;

namespace LevadS.Classes;

public abstract class HandlerBuilderBase(ILevadSBuilder levadSBuilder) : ILevadSBuilder
{
    ILevadSBuilder ILevadSBuilder.RegisterServicesFromAssemblyContaining(Type type)
        => levadSBuilder.RegisterServicesFromAssemblyContaining(type);

    IMessageHandlerBuilder<TMessage> ILevadSBuilder.AddMessageHandler<TMessage, THandler>(string topicPattern, Func<IMessageContext<TMessage>, THandler>? handlerFactory)
        => levadSBuilder.AddMessageHandler(topicPattern, handlerFactory);

    IRequestHandlerBuilder<TRequest, TResponse> ILevadSBuilder.AddRequestHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IRequestContext<TRequest>, THandler>? handlerFactory)
        => levadSBuilder.AddRequestHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);

    IStreamHandlerBuilder<TRequest, TResponse> ILevadSBuilder.AddStreamHandler<TRequest, TResponse, THandler>(string topicPattern, Func<IStreamContext<TRequest>, THandler>? handlerFactory)
        => levadSBuilder.AddStreamHandler<TRequest, TResponse, THandler>(topicPattern, handlerFactory);

    ILevadSBuilder ILevadSBuilder.AddMessageFilter<TMessage, TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory)
        => levadSBuilder.AddMessageFilter<TMessage, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddRequestFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddRequestFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddRequestFilter<TRequest, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddRequestFilter<TRequest, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddStreamFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddStreamFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddStreamFilter<TRequest, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddStreamFilter<TRequest, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddMessageDispatchFilter<TMessage, TFilter>(string topicPattern, Func<IMessageContext<TMessage>, TFilter>? filterFactory)
        => levadSBuilder.AddMessageDispatchFilter<TMessage, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddRequestDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddRequestDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IRequestContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddRequestDispatchFilter<TRequest, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddStreamDispatchFilter<TRequest, TResponse, TFilter>(topicPattern, filterFactory);

    ILevadSBuilder ILevadSBuilder.AddStreamDispatchFilter<TRequest, TFilter>(string topicPattern, Func<IStreamContext<TRequest>, TFilter>? filterFactory)
        => levadSBuilder.AddStreamDispatchFilter<TRequest, TFilter>(topicPattern, filterFactory);

    // ILevadSBuilder ILevadSBuilder.AddLevada<TLevada>(string levadaName, Action<ILevadaBuilder<TLevada>> builder)
    //     => levadSBuilder.AddLevada(levadaName, builder);

    // public ILevadSBuilder WarmUpMessageHandling<TMessage>()
    //     => levadSBuilder.WarmUpMessageHandling<TMessage>();
    //
    // public ILevadSBuilder WarmUpRequestHandling<TRequest, TResponse>()
    //     => levadSBuilder.WarmUpRequestHandling<TRequest, TResponse>();
    //
    // public ILevadSBuilder WarmUpStreamHandling<TRequest, TResponse>()
    //     => levadSBuilder.WarmUpStreamHandling<TRequest, TResponse>();
}
namespace LevadS.Interfaces;

internal interface ILevadaBuilder<out TLevada>
    where TLevada : ILevada
{
    IConfiguredLevadaBuilder Configure(Action<IServiceProvider, TLevada> configurationAction);
    IConfiguredLevadaBuilder Configure(Func<IServiceProvider, TLevada, Task> configurationActionAsync);
}

internal interface IConfiguredLevadaBuilder
{
    /// <summary>
    /// Declares message type that will be received from levada, if sent with topic matching any of patterns
    /// </summary>
    /// <param name="topicPatterns"></param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    IConfiguredLevadaBuilder AddInlet<TMessage>(params string[] topicPatterns);
    
    /// <summary>
    /// Declares message type that will be received from levada
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    IConfiguredLevadaBuilder AddInlet<TMessage>()
        => AddInlet<TMessage>("*");
    
    /// <summary>
    /// Declares message type that will be forwarded to levada, if sent with topic matching any of patterns
    /// </summary>
    /// <param name="topicPatterns"></param>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    IConfiguredLevadaBuilder AddOutlet<TMessage>(params string[] topicPatterns);
    
    /// <summary>
    /// Declares message type that will be forwarded to levada
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <returns></returns>
    IConfiguredLevadaBuilder AddOutlet<TMessage>()
        => AddOutlet<TMessage>("*");
}
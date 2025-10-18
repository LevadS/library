using LevadS.Classes.Extensions;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

public class MessageHandlerBuilder<TMessage>(ILevadSBuilder levadSBuilder, IServiceCollection serviceCollection, string key)
    : HandlerBuilderBase(levadSBuilder), IMessageHandlerBuilder<TMessage>
{
    public virtual IMessageHandlerBuilder<TMessage> WithFilter<TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IMessageHandlingFilter<TMessage>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        serviceCollection.AddKeyedTransientTopicMessageHandlingFilter<TMessage, TFilter>(topicPattern, key, h => filterFactory(h.ServiceProvider));

        return this;
    }
}
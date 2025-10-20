using LevadS.Interfaces;
using LevadS.Classes.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

internal class StreamHandlerBuilder<TRequest, TResponse>(ILevadSBuilder levadSBuilder, IServiceCollection serviceCollection, string key)
    : HandlerBuilderBase(levadSBuilder), IStreamHandlerBuilder<TRequest, TResponse>
{
    public virtual IStreamHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IStreamHandlingFilter<TRequest, TResponse>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        serviceCollection.AddKeyedTransientTopicStreamHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, key, h => filterFactory(h.ServiceProvider));

        return this;
    }
}
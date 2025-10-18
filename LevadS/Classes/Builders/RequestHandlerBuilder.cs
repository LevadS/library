using LevadS.Interfaces;
using LevadS.Classes.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Classes;

internal class RequestHandlerBuilder<TRequest, TResponse>(ILevadSBuilder levadSBuilder, IServiceCollection serviceCollection, string key)
    : HandlerBuilderBase(levadSBuilder), IRequestHandlerBuilder<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public virtual IRequestHandlerBuilder<TRequest, TResponse> WithFilter<TFilter>(string topicPattern, Func<IServiceProvider, TFilter>? filterFactory)
        where TFilter : class, IRequestHandlingFilter<TRequest, TResponse>
    {
        filterFactory ??= serviceProvider => ActivatorUtilities.CreateInstance<TFilter>(serviceProvider);

        serviceCollection.AddKeyedTransientTopicRequestHandlingFilter<TRequest, TResponse, TFilter>(topicPattern, key, h => filterFactory(h.ServiceProvider));

        return this;
    }
}
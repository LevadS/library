using BenchmarkDotNet.Attributes;
using LevadS.Benchmarks.Classes;
using LevadS.Benchmarks.Handlers;
using LevadS.Benchmarks.Messages;
using LevadS.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LevadS.Benchmarks.Benchmarks;

public class SimpleMessageHandling : BenchmarkBase
{
    public override void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddTransient<SimpleMessageHandler>()
            .AddLogging()
            .AddMediatR(c => c
                .RegisterServicesFromAssemblyContaining<SimpleMessageHandler>()
            )
            ;
    }

    public override void ConfigureLevadS(ILevadSBuilder builder)
    {
        builder
            .AddMessageHandler<SimpleMessage, SimpleMessageHandler>()
            // .WarmUpMessageHandling<SimpleMessage>()
            ;
    }
    
    private readonly IDispatcher _dispatcher;
    
    private readonly IMediator _mediator;

    public SimpleMessageHandling() : base()
    {
        _dispatcher = ServiceProvider.GetRequiredService<IDispatcher>();
        _mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task DispatchViaLevadS()
        => _dispatcher.SendAsync(new SimpleMessage());
    
    [Benchmark]
    public Task DispatchViaMediatR()
        => _mediator.Send(new SimpleMessage());
}
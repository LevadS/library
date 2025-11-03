using System.Text;
using System.Diagnostics;
using LevadS;
using LevadS.Classes;
using LevadS.Examples.ExceptionHandlers;
using LevadS.Examples.Filters;
using LevadS.Examples.Handlers;
using LevadS.Examples.Messages;
using LevadS.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddLevadS(b => b
    .RegisterServicesFromAssemblyContaining<StringHandler>()
    
    .AddMessageFilter<string, StringFilter>()
    
    .AddStreamExceptionHandler<IntStreamRequest, int, Exception>((ctx, callback) =>
    {
        callback(42);
        return Task.FromResult(true);
    })
    
    .AddMessageHandler<string>((string message) =>
    {
        Console.WriteLine(message);
    })
    .WithFilter((ctx, next) =>
    {
        Console.WriteLine(ctx.Message);
        return next();
    })
);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .MapGet(
        "/weatherforecast",
        async (IDispatcher dispatcher) => await dispatcher
            .RequestAsync<WeatherForecast>(
                new WeatherForecastRequest(),
                "topic",
                new Dictionary<string, object>() {{"x-tenant-id", "domainName"}}
            )
    )
    .WithDescription("Returns weather forecast");

app
    .MapGet(
        "/sendMessage/{topic}/{message}",
        (IDispatcher dispatcher, IServiceProvider serviceProvider, string message, string topic = "foo:bar") =>
        {
            var ctx = serviceProvider.GetService<IHttpContextAccessor>();
            return dispatcher.SendAsync(message, topic);
        })
    .WithDescription("Sends string message to single handler. Change topic to 'foo:42' to hit another handler");

app
    .MapGet(
        "/sendInheritedMessage",
        (IDispatcher dispatcher) => dispatcher.SendAsync(new InheritedMessage())
    )
    .WithDescription("Sends InheritedMessage to BaseMessage handler");

app
    .MapGet(
        "/sendInheritedRequest",
        async (IDispatcher dispatcher) => Debug.WriteLine((await dispatcher.RequestAsync(new InheritedRequest())).GetType().Name)
    )
    .WithDescription("Sends InheritedRequest to BaseRequest handler");

app
    .MapGet(
        "/sendWithException",
        (IDispatcher dispatcher) => dispatcher.SendAsync(42, "exception")
    )
    .WithDescription("Sends int to handler that throws an exception");

app
    .MapGet(
        "/publishMessage/{topic}/{message}",
        (IDispatcher dispatcher, string message, string topic = "foo:42") => dispatcher.PublishAsync(message, topic)
    )
    .WithDescription("Publishes string message to all handlers");

app
    .MapGet(
        "/subscribe",
        async (HttpContext httpContext, IHandlersRegister register) =>
        {
            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

            var handle = register
                .AddMessageHandler<string>(async (string message) =>
                {
                    Debug.WriteLine("Scoped message handler");
                    
                    await httpContext.Response.WriteAsync($"event: string\n");
                    await httpContext.Response.WriteAsync($"data: ");
                    await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message));
                    await httpContext.Response.WriteAsync($"\n\n");
                    await httpContext.Response.Body.FlushAsync();
                })
                .Build();

            await using (handle)
            {
                while (!httpContext.RequestAborted.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                }
            }

            return Results.Empty;
        }
    )
    .WithDescription("Endpoint returns SSE stream based on published string messages");

app
    .MapGet(
        "/stringStream",
        async (HttpContext httpContext, IDispatcher dispatcher) =>
        {
            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

            await foreach (var message in dispatcher.StreamAsync(new StringStreamRequest()))
            {
                await httpContext.Response.WriteAsync($"event: string\n");
                await httpContext.Response.WriteAsync($"data: ");
                await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message));
                await httpContext.Response.WriteAsync($"\n\n");
                await httpContext.Response.Body.FlushAsync();
            }

            return Results.Empty;
        }
    )
    .WithDescription("Endpoint returns SSE stream based on string stream response");

app
    .MapGet(
        "/intStream",
        async (HttpContext httpContext, IDispatcher dispatcher) =>
        {
            httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

            await foreach (var message in dispatcher.StreamAsync(new IntStreamRequest()))
            {
                await httpContext.Response.WriteAsync($"event: string\n");
                await httpContext.Response.WriteAsync($"data: ");
                await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message.ToString()));
                await httpContext.Response.WriteAsync($"\n\n");
                await httpContext.Response.Body.FlushAsync();
            }

            return Results.Empty;
        }
    )
    .WithDescription("Endpoint returns SSE stream based on int stream response");

app.Run();

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using LevadS;
using LevadS.Examples.Messages;
using LevadS.Examples.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<OrderRepository>();

builder.Services.AddLevadS(b => b
    .RegisterServicesFromAssemblyContaining<Order>()
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

app.MapPost(
    "/orders",
    async ([FromBody] Order order, IDispatcher dispatcher)
        => await dispatcher.SendAsync(order, "orders:create")
);

app.MapPut(
    "/orders/{orderId:int}",
    async ([FromRoute] int orderId, [FromBody] Order order, IDispatcher dispatcher)
        => await dispatcher.SendAsync(order with { Id = orderId }, "orders:update")
);

app.MapGet(
    "/orders/{orderId:int}",
    async ([FromRoute] int orderId, IDispatcher dispatcher)
        => await dispatcher.RequestAsync(new OrderRequest(orderId))  
);

app.MapGet(
    "/subscribe",
    async (HttpContext httpContext, IHandlersRegister register) =>
    {
        httpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

        var handle = register
            .AddMessageHandler<string>("notifications", async (string message) =>
            {
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
);

app.Run();

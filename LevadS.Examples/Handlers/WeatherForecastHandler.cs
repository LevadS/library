using LevadS.Attributes;
using LevadS.Interfaces;
using LevadS.Examples.Messages;

namespace LevadS.Examples.Handlers;

[LevadSRegistration]
class WeatherForecastHandler : IRequestHandler<WeatherForecastRequest, WeatherForecast>
{
    private readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    public Task<WeatherForecast> HandleAsync(IRequestContext<WeatherForecastRequest> requestContext)
        => Task.FromResult(
            new WeatherForecast
            (
                "",
                DateOnly.FromDateTime(DateTime.Now),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            )
        );
}
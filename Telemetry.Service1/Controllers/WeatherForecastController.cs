using Microsoft.AspNetCore.Mvc;

namespace Telemetry.Service1
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
    {
        readonly HttpClient httpClient = new();
        private readonly ILogger<WeatherForecastController> _logger = logger;

        [HttpGet, Route("5")]
        public IEnumerable<WeatherForecast> Get5WeatherForecasts()
        {
            _logger.LogInformation("Get5WeatherForecasts");

            httpClient.GetAsync("https://bora.work");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
            .ToArray();
        }

        [HttpGet, Route("10")]
        public IEnumerable<WeatherForecast> Get10WeatherForecasts()
        {
            _logger.LogInformation("Get10WeatherForecasts");
            httpClient.GetAsync("https://inexistente.org");
            return Enumerable.Range(1, 10).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
            .ToArray();
        }
    }
}

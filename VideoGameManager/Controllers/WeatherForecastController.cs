using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoGameManager.Controllers
{
    [ApiController]
    [Route("api/idojaras")] // https://localhost:5001/api/idojaras default: "[controller]" >>> https://localhost:5001/ZoliWeatherForecast
    // .....Controller - naming convention
    public class ZoliWeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ZoliWeatherForecastController> _logger;

        public ZoliWeatherForecastController(ILogger<ZoliWeatherForecastController> logger)
        {
            _logger = logger;
        }

        // Define Web Api endpoints
        [HttpPost] // This ia important, not the methodname!
        public string Brekekke()
        {
            return "Hello World!";
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Kakukk()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

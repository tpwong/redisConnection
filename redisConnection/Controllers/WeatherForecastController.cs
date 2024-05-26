using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace redisConnection.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IRedisHelper redisHelper;
        private readonly RedisDistributedSemaphore redisDistributed;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(IRedisHelper redisHelper, ILogger<WeatherForecastController> logger, RedisDistributedSemaphore redisDistributed)
        {
            _logger = logger;
            this.redisHelper = redisHelper;
            this.redisDistributed = redisDistributed;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            bool isAcquired = await redisDistributed.TryAcquireAsync();
            if (isAcquired)
            {
                try
                {
                    Console.WriteLine("Acquired semaphore, executing request...");
                    // Ö´ÐÐÒµÎñÂß¼­
                }
                finally
                {
                    await redisDistributed.ReleaseAsync();
                    Console.WriteLine("Semaphore released.");
                }
            }
            else
            {
                Console.WriteLine("Failed to acquire semaphore after retries, try later.");
            }


            await redisHelper.SetKey("211", "222124544771@aaaa");
            var tt = await redisHelper.GetKey("211");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

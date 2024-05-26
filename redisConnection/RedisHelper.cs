using StackExchange.Redis;
using System.Diagnostics;

namespace redisConnection
{
    public interface IRedisHelper
    {
        Task SetKey(string key, string value);
        Task<string> GetKey(string key);
    }
    public class RedisHelper() : IRedisHelper
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisHelper()
        {
            // Connection setup
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { "192.168.0.138:6379" },
            };

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;
        public static IDatabase database => Connection.GetDatabase();

        public async Task SetKey(string key, string value)
        {
            ThreadPool.SetMinThreads(3, 3);

            var sw = Stopwatch.StartNew();

            await Task.WhenAll(Enumerable.Range(0, 10)
                .Select(_ => Task.Run(async () =>
                {
                    await database.StringSetAsync(key, value);

                    Thread.Sleep(1000);
                })));

            Console.WriteLine(Connection.GetCounters().Interactive.PendingUnsentItems);
            Console.WriteLine(Connection.GetCounters().Interactive.SentItemsAwaitingResponse);
            Console.WriteLine(Connection.GetCounters().Interactive.ResponsesAwaitingAsyncCompletion);

            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        public async Task<string> GetKey(string key)
        {
            ThreadPool.SetMinThreads(3, 3);

            var sw = Stopwatch.StartNew();

            return await database.StringGetAsync(key);
        }
    }
}

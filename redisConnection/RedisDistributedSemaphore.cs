using StackExchange.Redis;
using System.Diagnostics;

namespace redisConnection
{
    public class RedisDistributedSemaphore
    {
        private readonly IDatabase _db;
        private readonly string _semaphoreKey;
        private readonly int _maxCount;
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(300);
        private readonly TimeSpan retryInterval = TimeSpan.FromSeconds(0.1);

        public RedisDistributedSemaphore()
        {
            var redis = ConnectionMultiplexer.Connect("192.168.56.1:6379");
            _db = redis.GetDatabase();
            _semaphoreKey = "globalSemaphore";
            _maxCount = 50;
        }

        public async Task<bool> TryAcquireAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            const string luaScript = @"
                local key = KEYS[1]
                local max = tonumber(ARGV[1])
                local current = tonumber(redis.call('get', key) or '0')
                if current < max then
                    redis.call('incr', key)
                    return 1
                else
                    return 0
                end";

            while (stopwatch.Elapsed < timeout)
            {
                bool acquired = (long)(await _db.ScriptEvaluateAsync(luaScript, [_semaphoreKey], [_maxCount])) == 1;
                if (acquired)
                {
                    return true;
                }
                await Task.Delay(retryInterval);  // 等待后重试
            }

            return false;  // 超时返回失败
        }

        public async Task ReleaseAsync()
        {
            await _db.ScriptEvaluateAsync("redis.call('decr', KEYS[1])", [_semaphoreKey]);
        }
    }
}

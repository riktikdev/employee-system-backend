using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace EmployeeApi.Utils
{
    public class RedisCache
    {
        private readonly IDatabase _cache;

        public RedisCache(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _cache.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T item, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(item);
            await _cache.StringSetAsync(key, json, expiry);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }
    }
}

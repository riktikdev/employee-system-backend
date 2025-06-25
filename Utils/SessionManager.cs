using System;
using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace EmployeeApi.Utils
{
    public class SessionData
    {
        public int UserId { get; set; }
        public string Role { get; set; }
    }

    public class SessionManager
    {
        private readonly IDatabase _db;
        private readonly TimeSpan _sessionExpiry = TimeSpan.FromHours(8); // 8 годин сесія

        public SessionManager(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task CreateSessionAsync(string token, int userId, string role)
        {
            var data = new SessionData
            {
                UserId = userId,
                Role = role
            };
            var json = JsonSerializer.Serialize(data);
            await _db.StringSetAsync(GetKey(token), json, _sessionExpiry);
        }

        public async Task<SessionData> GetSessionAsync(string token)
        {
            var value = await _db.StringGetAsync(GetKey(token));
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<SessionData>(value);
        }

        public async Task DeleteSessionAsync(string token)
        {
            await _db.KeyDeleteAsync(GetKey(token));
        }

        private string GetKey(string token) => $"session:{token}";
    }
}

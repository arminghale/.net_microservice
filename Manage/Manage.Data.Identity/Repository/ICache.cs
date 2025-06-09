using Manage.Data.Identity.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Manage.Data.Identity.Repository
{
    public interface ICache
    {
        T? GetData<T>(string key, int expirationMin=-1);

        bool SetData<T>(string key, T value, int expirationMin);

        object RemoveData(string key);
    }

    public class CacheService : ICache
    {
        private IDatabase _db;
        public CacheService(string redisConfig)
        {
            _db = ConnectionMultiplexer.Connect(redisConfig).GetDatabase();
        }

        public T? GetData<T>(string key, int expirationMin=-1)
        {
            RedisValue value;
            if (expirationMin>0)
            {
                TimeSpan expiryTime = DateTimeOffset.Now.AddMinutes((double)expirationMin).DateTime.Subtract(DateTime.Now);
                value = _db.StringGetSetExpiry(key, expiryTime);
            }
            else
            {
                value = _db.StringGet(key);
            }
            if (!value.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }

        public object RemoveData(string key)
        {
            bool _isKeyExist = _db.KeyExists(key);
            if (_isKeyExist == true)
            {
                return _db.KeyDelete(key);
            }
            return false;
        }

        public bool SetData<T>(string key, T value, int expirationMin)
        {
            TimeSpan expiryTime = DateTimeOffset.Now.AddMinutes((double)expirationMin).DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
            return isSet;
        }
    }
}

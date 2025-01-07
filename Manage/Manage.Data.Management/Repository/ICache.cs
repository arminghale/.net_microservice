using Manage.Data.Management.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace Manage.Data.Management.Repository
{
    public interface ICache
    {
        T? GetData<T>(string key);

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

        public T? GetData<T>(string key)
        {
            var value = _db.StringGet(key);
            if (!string.IsNullOrEmpty(value))
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
            TimeSpan expiryTime = DateTimeOffset.Now.AddMinutes(5.0).DateTime.Subtract(DateTime.Now);
            var isSet = _db.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
            return isSet;
        }
    }
}

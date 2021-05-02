using discordbot.DAL.Infrastructure.Interfaces;

using Newtonsoft.Json;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.DAL.Infrastructure
{
    public class RedisDbContext : IDbContext
    {
        /*
         * value set for each type  : keys:{typeName}
         * value cache key          : keys:{typeName}:{key}
         */
        private const string value_set_cacheKeyTemplate = "keys:{0}";
        private const string value_cacheKeyTemplate = "keys:{0}:{1}";
        private readonly IRedisConnectionFactory redisConnectionFactory;

        public RedisDbContext(IRedisConnectionFactory redisConnectionFactory)
        {
            this.redisConnectionFactory = redisConnectionFactory;
        }

        private IDatabase GetRedisDatabse()
        {
            return this.redisConnectionFactory.Connection().GetDatabase();
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : TValue
        {
            string value_set_cacheKey = string.Format(value_set_cacheKeyTemplate, typeof(T).Name);
            string[] keys = (await this.GetRedisDatabse().SetMembersAsync(value_set_cacheKey)).ToStringArray();
            IEnumerable<T> values = (await this.GetRedisDatabse().StringGetAsync(keys.ToRedisKeys().ToArray()))
                                    .Select(v => JsonConvert.DeserializeObject<T>(v));
            return values;
        }

        public async Task<T> Get<T>(string key) where T : TValue
        {
            string value_cacheKey = string.Format(value_cacheKeyTemplate, typeof(T).Name, key);
            T value = JsonConvert.DeserializeObject<T>(await this.GetRedisDatabse().StringGetAsync(value_cacheKey));
            return value;
        }

        public async Task<IEnumerable<T>> Gets<T>(IEnumerable<string> keys) where T : TValue
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Set<T>(TValue value) where T : TValue
        {
            string value_set_cacheKey = string.Format(value_set_cacheKeyTemplate, typeof(T).Name);
            string value_cacheKey = string.Format(value_cacheKeyTemplate, typeof(T).Name, value.Key);
            bool result = await this.GetRedisDatabse().StringSetAsync(value_cacheKey, JsonConvert.SerializeObject(value.Value));
            if (!result)
            {
                return false;
            }
            result = this.GetRedisDatabse().SetAdd(value_set_cacheKey, value_cacheKey);
            return result;
        }

        public async Task<int> Sets<T>(IEnumerable<TValue> values) where T : TValue
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Delete(TValue document)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}

using discordbot.DAL.Infrastructure.Interfaces;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System;

namespace discordbot.DAL.Infrastructure
{
    public class RedisConfiguration
    {
        public string ConnectionString { get; set; }
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        /// <summary>
        /// The redis connection.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connection;

        public RedisConnectionFactory(IOptions<RedisConfiguration> redis)
        {
            this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redis.Value.ConnectionString));
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection.Value;
        }
    }
}

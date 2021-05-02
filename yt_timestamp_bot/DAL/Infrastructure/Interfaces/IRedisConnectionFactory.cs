using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.DAL.Infrastructure.Interfaces
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connection();
    }
}

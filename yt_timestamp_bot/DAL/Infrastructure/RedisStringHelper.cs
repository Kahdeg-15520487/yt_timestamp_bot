using StackExchange.Redis;

using System;
using System.Collections.Generic;

namespace discordbot.DAL.Infrastructure
{
    public static class RedisStringHelper
    {
        public static IEnumerable<RedisKey> ToRedisKeys(this string[] values)
        {
            foreach (string v in values)
            {
                yield return v;
            }
        }

        public static IEnumerable<HashEntry> ToHashEntries(this IEnumerable<KeyValuePair<string, string>> fields)
        {
            foreach (KeyValuePair<string, string> kvp in fields)
            {
                yield return new HashEntry(kvp.Key, kvp.Value);
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs(this IEnumerable<HashEntry> hashEntries)
        {
            foreach (HashEntry he in hashEntries)
            {
                yield return new KeyValuePair<string, string>(he.Name, he.Value);
            }
        }

        public static IEnumerable<SortedSetEntry> ToSortedSetEntries(this string[] members, Func<string, double> scoreCalc)
        {
            foreach (string m in members)
            {
                yield return new SortedSetEntry(m, scoreCalc(m));
            }
        }
    }
}

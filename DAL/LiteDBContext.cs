using LiteDB;

using Microsoft.Extensions.Options;

using System;
using System.IO;

namespace discordbot.DAL
{
    internal class LiteDbConfig
    {
        public string ConnectionString { get; set; }
    }
    internal class LiteDbContext
    {
        public readonly ILiteDatabase Context;
        public LiteDbContext(IOptions<LiteDbConfig> configs)
        {
            Console.WriteLine(Path.GetFullPath(configs.Value.ConnectionString));
            try
            {
                LiteDatabase db = new LiteDatabase(configs.Value.ConnectionString);
                if (db != null)
                    Context = db;
            }
            catch (Exception ex)
            {
                throw new Exception("Can find or create LiteDb database.", ex);
            }
        }
    }
}

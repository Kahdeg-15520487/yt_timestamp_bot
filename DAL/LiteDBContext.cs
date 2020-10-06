using LiteDB;

using Microsoft.Extensions.Options;

using System;

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
            Console.WriteLine(configs.Value.ConnectionString);
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

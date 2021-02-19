using LiteDB;

using Microsoft.Extensions.Options;

using System;
using System.IO;
using System.Text;

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
                {
                    Context = db;
                    StringBuilder sb = new StringBuilder();
                    foreach (var collectionName in db.GetCollectionNames())
                    {
                        sb.AppendLine("=====");
                        sb.AppendLine(collectionName);
                        sb.AppendLine(JsonSerializer.Serialize(new BsonArray(db.GetCollection(collectionName).FindAll())));
                        sb.AppendLine("=====");
                    }
                    File.WriteAllText(Guid.NewGuid().ToString(), sb.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Can find or create LiteDb database.", ex);
            }
        }
    }
}

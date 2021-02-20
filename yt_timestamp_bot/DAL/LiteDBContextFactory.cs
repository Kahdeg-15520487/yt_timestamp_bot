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
    internal class LiteDBContextFactory
    {
        private readonly IOptions<LiteDbConfig> configs;
        private LiteDatabase db = null;
        public LiteDBContextFactory(IOptions<LiteDbConfig> configs)
        {
            this.configs = configs;
        }

        public ILiteDatabase GetDatabase()
        {
            Console.Write((new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().DeclaringType?.Name ?? "unknown");
            Console.Write(" requested db: ");
            Console.WriteLine(Path.GetFullPath(configs.Value.ConnectionString));

            try
            {
                if (this.db != null)
                {
                    this.db.Dispose();
                }
                this.db = new LiteDatabase(configs.Value.ConnectionString);
                if (db != null)
                {
                    return db;
                    //StringBuilder sb = new StringBuilder();
                    //foreach (var collectionName in db.GetCollectionNames())
                    //{
                    //    sb.AppendLine("=====");
                    //    sb.AppendLine(collectionName);
                    //    sb.AppendLine(JsonSerializer.Serialize(new BsonArray(db.GetCollection(collectionName).FindAll())));
                    //    sb.AppendLine("=====");
                    //}
                    //File.WriteAllText(Guid.NewGuid().ToString(), sb.ToString());
                }
                else
                {
                    throw new NullReferenceException(nameof(LiteDatabase));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Can find or create LiteDb database.", ex);
            }
        }
    }
}

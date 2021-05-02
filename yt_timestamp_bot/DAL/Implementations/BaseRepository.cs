using discordbot.DAL.Entities;
using discordbot.DAL.Infrastructure.Interfaces;
using discordbot.DAL.Interfaces;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace discordbot.DAL.Implementations
{
    class BaseRepository<T> : IBaseRepository<T> where T : TValue
    {
        protected readonly IDbContext db;

        public BaseRepository(IDbContext db)
        {
            this.db = db;
        }

        ~BaseRepository()
        {
            this.db.Dispose();
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await db.GetAll<T>();
        }

        public virtual async Task<T> Get(string id)
        {
            return await db.Get<T>(id);
        }

        public virtual async Task<IEnumerable<T>> Query(Func<T, bool> predicate)
        {
            return (await this.GetAll()).Where(predicate);
        }

        public virtual async Task<bool> Save(T document)
        {
            return await db.Set<T>(document);
        }

        public virtual async Task<bool> Delete(T document)
        {
            return await db.Delete(document);
        }
    }
}

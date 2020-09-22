using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace discordbot.DAL.Implementations
{
    class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly ILiteDatabase db;

        public BaseRepository(ILiteDatabase db)
        {
            this.db = db;
        }

        public virtual IEnumerable<T> GetAll()
        {
            ILiteCollection<T> collection = db.GetCollection<T>();
            return collection.FindAll().ToList();
        }

        public virtual T Get(ObjectId id)
        {
            ILiteCollection<T> collection = db.GetCollection<T>();
            T get = collection.FindOne(x => x.Id == id);
            return get;
        }

        public virtual IEnumerable<T> Query(Expression<Func<T, bool>> predicate)
        {
            return db.GetCollection<T>().Find(predicate);
        }

        public virtual ObjectId Save(T document)
        {
            ILiteCollection<T> collection = db.GetCollection<T>();
            ObjectId result = null;

            if (!collection.Exists(x => x.Id == document.Id))
            {
                result = collection.Insert(document);
            }
            else
            {
                collection.Update(document);
                result = document.Id;
            }
            return result;
        }

        public virtual void Delete(T document)
        {
            ILiteCollection<T> collection = db.GetCollection<T>();
            collection.DeleteMany(x => x.Id == document.Id);
        }
    }
}

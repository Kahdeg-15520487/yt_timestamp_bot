using discordbot.DAL.Entities;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace discordbot.DAL.Interfaces
{

    interface IBaseRepository<T> where T : BaseEntity
    {
        IEnumerable<T> GetAll();
        T Get(ObjectId id);
        IEnumerable<T> Query(Expression<Func<T, bool>> predicate);
        ObjectId Save(T document);
        void Delete(T document);
    }
}

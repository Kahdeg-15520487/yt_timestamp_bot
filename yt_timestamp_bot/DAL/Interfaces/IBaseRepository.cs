using discordbot.DAL.Infrastructure.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace discordbot.DAL.Interfaces
{

    interface IBaseRepository<T> where T : TValue
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(string id);
        Task<IEnumerable<T>> Query(Func<T, bool> predicate);
        Task<bool> Save(T document);
        Task<bool> Delete(T document);
    }
}

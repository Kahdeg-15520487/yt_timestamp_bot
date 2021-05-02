using discordbot.DAL.Entities;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace discordbot.DAL.Interfaces
{
    public interface IVideoRepository
    {
        Task<Video> GetVideo(string videoId);
        Task<IEnumerable<Video>> GetAll();
        Task<IEnumerable<Video>> Query(Func<Video, bool> predicate);
        Task<bool> Save(Video document);
        Task<bool> Delete(Video document);
    }
}

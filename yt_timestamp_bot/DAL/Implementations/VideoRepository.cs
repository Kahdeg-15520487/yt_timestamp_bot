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
    public class VideoRepository : IVideoRepository
    {
        private readonly IDbContext db;
        public VideoRepository(IDbContext db)
        {
            this.db = db;
        }

        ~VideoRepository()
        {
            this.db.Dispose();
        }

        public async Task<IEnumerable<Video>> GetAll()
        {
            return await db.GetAll<Video>();
        }

        public async Task<Video> GetVideo(string videoId)
        {
            return await db.Get<Video>(videoId);
        }

        public async Task<IEnumerable<Video>> Query(Func<Video, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Save(Video document)
        {
            return await db.Set<Video>(document);
        }

        public async Task<bool> Delete(Video document)
        {
            throw new NotImplementedException();
        }
    }
}

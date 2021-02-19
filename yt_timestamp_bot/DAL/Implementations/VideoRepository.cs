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
    class VideoRepository : IVideoRepository
    {
        private readonly ILiteDatabase db;
        public VideoRepository(LiteDbContext db)
        {
            this.db = db.Context;
        }

        public IEnumerable<Video> GetAll()
        {
            ILiteCollection<Video> collection = db.GetCollection<Video>();
            return collection.FindAll();
        }

        public Video GetVideo(string videoId)
        {
            return db.GetCollection<Video>().FindById(new BsonValue(videoId));
        }

        public string Save(Video document)
        {
            ILiteCollection<Video> collection = db.GetCollection<Video>();
            string result = null;

            if (!collection.Exists(x => x.VideoId == document.VideoId))
            {
                result = collection.Insert(document);
            }
            else
            {
                if (!collection.Update(document))
                {
                    return null;
                }
                result = document.VideoId;

            }
            return result;
        }

        public IEnumerable<Video> Query(Expression<Func<Video, bool>> predicate)
        {
            return db.GetCollection<Video>().Find(predicate);
        }

        public bool Delete(Video document)
        {
            return db.GetCollection<Video>().Delete(new BsonValue(document.VideoId));
        }
    }
}

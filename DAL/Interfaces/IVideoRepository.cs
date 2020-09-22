using discordbot.DAL.Entities;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace discordbot.DAL.Interfaces
{
    interface IVideoRepository
    {
        Video GetVideo(string videoId);
        IEnumerable<Video> GetAll();
        IEnumerable<Video> Query(Expression<Func<Video, bool>> predicate);
        string Save(Video document);
        bool Delete(Video document);
    }
}

using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

using Google.Apis.Logging;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discordbot.Services
{
    class TagService : ITagService
    {
        private readonly ITimeStampRepository db;
        private readonly ILogger<TagService> logger;
        private readonly YoutubeInterface ytInterface;

        private static Livestream currentLiveStream;
        public Livestream CurrentLiveStream { get => currentLiveStream; }

        public TagService(ITimeStampRepository tsDb, ILogger<TagService> logger, YoutubeInterface ytInterface)
        {
            this.db = tsDb;
            this.logger = logger;
            this.ytInterface = ytInterface;
        }

        public async Task StartTag()
        {
            YouTubeService ytService = ytInterface.GetYoutubeService();
            string livestreamId = await ytInterface.GetLiveStream(ytService, "upcoming");
            currentLiveStream = ytInterface.GetLiveStreamInfo(livestreamId).Result;
        }

        public void EndTag()
        {
            currentLiveStream = null;
        }

        public Tag GetTag(string tag)
        {
            if (currentLiveStream != null)
            {
                TimeStamp timestamp = db.Query(ts => ts.TagContent.Equals(tag)).FirstOrDefault();
                return new Tag(timestamp);
            }
            return null;
        }

        public IEnumerable<Tag> ListTag()
        {
            if (currentLiveStream != null)
            {
                return db.Query(ts => ts.VideoId.Equals(currentLiveStream.VideoId))
                         .OrderBy(ts => ts.Time)
                         .Select(ts => new Tag(ts));
            }
            return null;
        }

        public IEnumerable<Tag> ListTag(string videoId)
        {
            var querry = db.Query(ts => ts.VideoId.Equals(videoId))
                           .OrderBy(ts => ts.Time)
                           .Select(ts => new Tag(ts));
            return querry.ToList();
        }

        public Tag AddTag(string tagContent, ulong userId, string userName)
        {
            if (currentLiveStream != null)
            {
                Tag tag = Tag.Create(tagContent, currentLiveStream.VideoId, currentLiveStream.StartTime, userId, userName);
                db.Save(new TimeStamp(tag));
                return tag;
            }
            return null;
        }

        public bool DeleteTag(string tag)
        {
            throw new NotImplementedException();
        }

        public Tag EditTag(ulong userId, string tagContent)
        {
            if (currentLiveStream != null)
            {
                TimeStamp timeStamp = db.Query(ts => ts.UserId == userId)
                              .OrderBy(ts => ts.Time)
                              .FirstOrDefault();
                var oldtag = new Tag(timeStamp);
                if (timeStamp == null)
                {
                    return null;
                }

                timeStamp.TagContent = tagContent;
                db.Save(timeStamp);
                return oldtag;
            }
            return null;
        }

        public DateTime RecalculateTag(DateTime dt, string videoId = null)
        {
            IEnumerable<TimeStamp> timeStamps = videoId == null ?
                db.Query(ts => ts.VideoId.Equals(currentLiveStream.VideoId))
                :
                db.Query(ts => ts.VideoId.Equals(videoId))
                ;
            DateTime oldTime = default;
            foreach (TimeStamp timeStamp in timeStamps)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(timeStamp.Time);
                oldTime = timeStamp.ActualTime - timeSpan;
                timeStamp.Time = (timeStamp.ActualTime - dt).TotalSeconds;
                db.Save(timeStamp);
            }
            return oldTime;
        }
    }
}

using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

using Google.Apis.Logging;
using Google.Apis.YouTube.v3;

using LiteDB;

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

        private static VideoDto currentLiveStream;
        public VideoDto CurrentLiveStream { get => currentLiveStream; }
        public bool IsLive { get => currentLiveStream != null; }

        public TagService(ITimeStampRepository tsDb, ILogger<TagService> logger, YoutubeInterface ytInterface)
        {
            this.db = tsDb;
            this.logger = logger;
            this.ytInterface = ytInterface;
        }

        public async Task<bool> StartTag()
        {
            //todo replace with a mechanism of auto stream grabbing
            YouTubeService ytService = ytInterface.GetYoutubeService();
            try
            {
                string livestreamId = await ytInterface.GetLiveStream(ytService, "Upcoming");
                if (livestreamId == null)
                {
                    logger.LogInformation("null");
                    livestreamId = await ytInterface.GetLiveStream(ytService, "Live");
                }
                currentLiveStream = ytInterface.GetVideoInfo(ytService, livestreamId).Result;
                logger.LogInformation($"starting tagging for {livestreamId}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(StartTag));
                logger.LogInformation("found no upcoming stream");
                return false;
            }
        }

        public void EndTag()
        {
            currentLiveStream = null;
        }

        public TimeStampDto GetTag(string tag)
        {
            if (currentLiveStream != null)
            {
                TimeStamp timestamp = db.Query(ts => ts.TagContent.Equals(tag)).FirstOrDefault();
                return new TimeStampDto(timestamp);
            }
            return null;
        }

        public IEnumerable<TimeStampDto> ListTag()
        {
            if (currentLiveStream != null)
            {
                return db.Query(ts => ts.VideoId.Equals(currentLiveStream.VideoId))
                         .OrderBy(ts => ts.Time)
                         .Select(ts => new TimeStampDto(ts));
            }
            return null;
        }

        public IEnumerable<TimeStampDto> ListTag(string videoId)
        {
            IEnumerable<TimeStampDto> querry = db.Query(ts => ts.VideoId.Equals(videoId))
                           .OrderBy(ts => ts.Time)
                           .Select(ts => new TimeStampDto(ts));
            return querry.ToList();
        }

        public TimeStampDto AddTag(string tagContent, ulong userId, string userName)
        {
            if (currentLiveStream != null)
            {
                TimeStamp ts = new TimeStamp(tagContent, currentLiveStream.VideoId, currentLiveStream.StartTime, userId, userName);
                ObjectId id = db.Save(ts);
                ts.Id = id;
                return new TimeStampDto(ts);
            }
            return null;
        }

        public bool DeleteTag(string tag)
        {
            throw new NotImplementedException();
        }

        public TimeStampDto EditTag(ulong userId, string tagContent)
        {
            if (currentLiveStream != null)
            {
                TimeStamp timeStamp = db.Query(ts => ts.UserId == userId)
                              .OrderBy(ts => ts.Time)
                              .FirstOrDefault();
                TimeStampDto oldtag = new TimeStampDto(timeStamp);
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
            if (true)
            {
                //todo ensure videoId exist
            }

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

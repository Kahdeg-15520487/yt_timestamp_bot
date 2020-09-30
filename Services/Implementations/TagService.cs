using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

using Google.Apis.Http;
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
        private readonly ITimeStampRepository tsDb;
        private readonly IVideoRepository videoDb;
        private readonly ILogger<TagService> logger;
        private readonly YoutubeInterface ytInterface;

        private static VideoDto currentLiveStream;
        public VideoDto CurrentLiveStream { get => currentLiveStream; }
        public bool IsLive { get => currentLiveStream != null; }

        public TagService(ITimeStampRepository tsDb, IVideoRepository videoDb, ILogger<TagService> logger, YoutubeInterface ytInterface)
        {
            this.tsDb = tsDb;
            this.videoDb = videoDb;
            this.logger = logger;
            this.ytInterface = ytInterface;
        }

        public async Task<bool> StartTag(string videoId = null)
        {
            //todo replace with a mechanism of auto stream grabbing
            YouTubeService ytService = ytInterface.GetYoutubeService();
            try
            {
                string livestreamId = videoId;
                if (livestreamId == null)
                {
                    livestreamId = await ytInterface.GetLiveStream(ytService, "Upcoming");
                    if (livestreamId == null)
                    {
                        logger.LogInformation("null");
                        livestreamId = await ytInterface.GetLiveStream(ytService, "Live");
                    }
                }
                livestreamId = await ytInterface.GetVideoId(livestreamId);
                try
                {
                    currentLiveStream = await ytInterface.GetVideoInfo(ytService, livestreamId);
                }
                catch (StartCapturingTooSoonException ex)
                {
                    logger.LogError(ex, "Start capture too soon");
                    return false;
                }

                Video v = new Video(currentLiveStream);
                if (videoDb.Save(v) == null)
                {
                    logger.LogInformation($"Can't save video object {livestreamId}");
                    return false;
                }

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

        public async Task EndTag()
        {
            if (currentLiveStream == null)
            {
                return;
            }

            YouTubeService ytService = ytInterface.GetYoutubeService();
            try
            {
                currentLiveStream = await ytInterface.GetVideoInfo(ytService, currentLiveStream.VideoId);
            }
            catch (StartCapturingTooSoonException ex)
            {
                logger.LogError(ex, "Start capture too soon");
            }

            Video v = new Video(currentLiveStream);
            if (videoDb.Save(v) == null)
            {
                logger.LogInformation($"Can't save video object {currentLiveStream.VideoId}");
            }

            currentLiveStream = null;
        }

        public TimeStampDto GetTag(string tag)
        {
            if (currentLiveStream != null)
            {
                TimeStamp timestamp = tsDb.Query(ts => ts.TagContent.Equals(tag)).FirstOrDefault();
                return new TimeStampDto(timestamp);
            }
            return null;
        }

        public List<TimeStampDto> ListTag(string videoId = null)
        {
            videoId = videoId == null ? currentLiveStream.VideoId : ytInterface.GetVideoId(videoId).Result;

            IEnumerable<TimeStampDto> querry = tsDb.Query(ts => ts.VideoId.Equals(videoId))
                           .OrderBy(ts => ts.Time)
                           .Select(ts => new TimeStampDto(ts));

            return querry.ToList();
        }

        public List<TimeStampDto> ListTag(string videoId, ulong userId)
        {
            IEnumerable<TimeStampDto> querry = tsDb.Query(ts => ts.VideoId.Equals(videoId) && ts.UserId == userId)
                           .OrderBy(ts => ts.Time)
                           .Select(ts => new TimeStampDto(ts));
            return querry.ToList();
        }

        public TimeStampDto AddTag(string tagContent, ulong userId, string userName, int second = 0)
        {
            if (currentLiveStream != null)
            {
                TimeStamp ts = new TimeStamp(tagContent, currentLiveStream.VideoId, currentLiveStream.StartTime, userId, userName, second);
                ObjectId id = tsDb.Save(ts);
                ts.Id = id;
                return new TimeStampDto(ts);
            }
            return null;
        }

        public TimeStampDto AddTag(string tagContent, ulong userId, string userName, DateTime actualTime)
        {
            if (currentLiveStream != null)
            {
                TimeStamp ts = new TimeStamp(tagContent, currentLiveStream.VideoId, currentLiveStream.StartTime, actualTime, userId, userName);
                ObjectId id = tsDb.Save(ts);
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
                TimeStamp timeStamp = tsDb.Query(ts => ts.UserId == userId && ts.VideoId == currentLiveStream.VideoId)
                              .OrderByDescending(ts => ts.LastModified)
                              .FirstOrDefault();
                TimeStampDto oldtag = new TimeStampDto(timeStamp);
                if (timeStamp == null)
                {
                    return null;
                }

                timeStamp.TagContent = tagContent;
                timeStamp.LastModified = DateTime.UtcNow;
                tsDb.Save(timeStamp);
                return oldtag;
            }
            return null;
        }

        public bool ShiftTag(int x, string videoId = null)
        {
            List<TimeStamp> timeStamps = (videoId == null ?
                tsDb.Query(ts => ts.VideoId.Equals(currentLiveStream.VideoId))
                :
                tsDb.Query(ts => ts.VideoId.Equals(videoId))
                ).ToList();

            if (timeStamps.Count == 0)
            {
                throw new KeyNotFoundException(videoId);
            }

            foreach (TimeStamp timeStamp in timeStamps)
            {
                timeStamp.ActualTime = timeStamp.ActualTime.AddSeconds(x);
                timeStamp.Time += x;
                tsDb.Save(timeStamp);
            }
            return true;
        }
    }
}

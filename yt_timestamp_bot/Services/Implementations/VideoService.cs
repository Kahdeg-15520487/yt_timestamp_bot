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

namespace discordbot.Services.Implementations
{
    class VideoService : IVideoService
    {
        private readonly ITimeStampRepository tsDb;
        private readonly IVideoRepository videoDb;
        private readonly ILogger<VideoService> logger;

        public VideoService(ITimeStampRepository tsDb, IVideoRepository videoDb, ILogger<VideoService> logger)
        {
            this.tsDb = tsDb;
            this.videoDb = videoDb;
            this.logger = logger;
        }

        public IEnumerable<TimeStampDto> GetTimeStamps(string videoId)
        {
            IEnumerable<TimeStampDto> query = tsDb.Query(ts => ts.VideoId.Equals(videoId))
                           .OrderBy(ts => ts.Time)
                           .Select(ts => new TimeStampDto(ts));

            return query.ToList();
        }

        public VideoDto GetVideo(string videoId)
        {
            Video entity = videoDb.Query(vd => vd.VideoId.Equals(videoId)).FirstOrDefault();
            VideoDto dto = new VideoDto(entity);
            dto.TimeStamps = GetTimeStamps(videoId);
            return dto;
        }

        public IEnumerable<VideoDto> ListVideos()
        {
            IEnumerable<VideoDto> query = videoDb.GetAll().OrderBy(vd => vd.StartTime).Select(vd => new VideoDto(vd));
            return query.ToList();
        }
    }
}

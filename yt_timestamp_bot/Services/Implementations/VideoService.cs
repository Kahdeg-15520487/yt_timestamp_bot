using discordbot.DAL.Entities;
using discordbot.DAL.Interfaces;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

using Google.Apis.Http;
using Google.Apis.Logging;
using Google.Apis.YouTube.v3;

using LiteDB;

using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<VideoService> logger;

        public VideoService(IServiceScopeFactory serviceScopeFactory, ILogger<VideoService> logger)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        public IEnumerable<TimeStampDto> GetTimeStamps(string videoId)
        {
            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                ITimeStampRepository tsDb = scope.ServiceProvider.GetRequiredService<ITimeStampRepository>();
                IEnumerable<TimeStampDto> query = tsDb.Query(ts => ts.VideoId.Equals(videoId))
                           .OrderBy(ts => ts.Time)
                           .Select(ts => new TimeStampDto(ts));

                return query.ToList();
            }
        }

        public VideoDto GetVideo(string videoId)
        {
            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                IVideoRepository videoDb = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
                Video entity = videoDb.Query(vd => vd.VideoId.Equals(videoId)).FirstOrDefault();
                if (entity == null)
                {
                    return null;
                }
                VideoDto dto = new VideoDto(entity);
                dto.TimeStamps = GetTimeStamps(videoId);
                return dto;
            }
        }

        public IEnumerable<VideoDto> ListVideos()
        {
            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                IVideoRepository videoDb = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
                List<VideoDto> query = videoDb.GetAll().OrderBy(vd => vd.StartTime).Select(vd => new VideoDto(vd)).ToList();


                ITimeStampRepository tsDb = scope.ServiceProvider.GetRequiredService<ITimeStampRepository>();
                return query.Select(vdto =>
                {
                    vdto.TimeStamps = tsDb.Query(ts => ts.VideoId.Equals(vdto.VideoId))
                                          .OrderBy(ts => ts.Time)
                                          .Select(ts => new TimeStampDto(ts));
                    return vdto;
                });
            }
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using discordbot.Services.DTOs;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace discordbot.Services
{
    class YoutubeInterface
    {
        private readonly string ApiKey;
        private readonly ILogger<YoutubeInterface> logger;

        public YoutubeInterface(IConfiguration configuration, ILogger<YoutubeInterface> logger)
        {
            ApiKey = configuration["_APIKEY"];
            this.logger = logger;
        }

        [Obsolete("use for oauth")]
        private async Task<UserCredential> Login()
        {
            using (FileStream stream = new FileStream("secret.json", FileMode.Open, FileAccess.Read))
            {

                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString()));
                return credential;
            }
        }

        internal YouTubeService GetYoutubeService()
        {
            return new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = ApiKey,
                ApplicationName = "TimeStampBot"
            });
        }

        private async Task<VideoDto> GetCurrentLiveStream()
        {
            YouTubeService t = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = ApiKey,
                ApplicationName = "TimeStampBot"
            });

            SearchResource.ListRequest request = t.Search.List("id");
            request.ChannelId = "UC_a1ZYZ8ZTXpjg9xUY9sj8w";
            request.EventType = SearchResource.ListRequest.EventTypeEnum.Live;
            request.MaxResults = 1;
            request.Type = "video";
            request.Fields = "items(id(videoId))";
            SearchListResponse result = await request.ExecuteAsync();
            SearchResult livestream = result.Items.FirstOrDefault();
            return new VideoDto()
            {
                VideoId = livestream?.Id.VideoId,
                StartTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(livestream.Snippet.PublishedAt))
            };
        }

        private async Task<VideoDto> GetUpcomingLiveStream()
        {
            YouTubeService t = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = ApiKey,
                ApplicationName = "TimeStampBot"
            });

            SearchResource.ListRequest request = t.Search.List("id");
            request.ChannelId = "UC_a1ZYZ8ZTXpjg9xUY9sj8w";
            request.EventType = SearchResource.ListRequest.EventTypeEnum.Live;
            request.MaxResults = 1;
            request.Type = "video";
            request.Fields = "items(id(videoId))";
            SearchListResponse result = await request.ExecuteAsync();
            SearchResult livestream = result.Items.FirstOrDefault();
            return new VideoDto()
            {
                VideoId = livestream?.Id.VideoId,
                StartTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(livestream.Snippet.PublishedAt))
            };
        }

        internal async Task<string> GetLiveStream(YouTubeService ytService, string eventType)
        {
            try
            {
                SearchResource.ListRequest request = ytService.Search.List("id");
                request.ChannelId = "UC_a1ZYZ8ZTXpjg9xUY9sj8w";
                request.EventType = eventType.ToEnum<SearchResource.ListRequest.EventTypeEnum>();
                request.MaxResults = 1;
                request.Type = "video";
                request.Fields = "items(id(videoId))";
                SearchListResponse result = await request.ExecuteAsync();
                SearchResult livestream = result.Items.FirstOrDefault();
                return livestream?.Id.VideoId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetLiveStream));
                return null;
            }
        }

        internal async Task<VideoDto> GetVideoInfo(YouTubeService ytService, string videoId)
        {
            try
            {
                VideosResource.ListRequest request = ytService.Videos.List("snippet,id,liveStreamingDetails");
                request.Id = videoId;
                request.Fields = "items(id,liveStreamingDetails(activeLiveChatId,concurrentViewers,scheduledStartTime,actualStartTime)," +
                                 "snippet(channelId,channelTitle,description,liveBroadcastContent,publishedAt,thumbnails,title),statistics)";
                VideoListResponse result = await request.ExecuteAsync();
                Video livestream = result.Items.FirstOrDefault();
                return new VideoDto()
                {
                    VideoId = livestream?.Id,
                    StartTime = DateTime.Parse(livestream.LiveStreamingDetails.ActualStartTime, styles: DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                    EndTime = DateTime.Parse(livestream.LiveStreamingDetails.ActualEndTime ?? DateTime.UtcNow.ToLongTimeString(), styles: DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetVideoInfo));
                return null;
            }
        }

        //internal async Task<Livestream> GetVideo(string videoId)
        //{
        //    YouTubeService t = new YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = await Login(),
        //        ApplicationName = "TimeStampBot"
        //    });


        //    VideosResource.ListRequest list = t.Videos.List("snippet");
        //    list.Id = videoId;
        //    VideoListResponse listResult = await list.ExecuteAsync();
        //    Video video = listResult.Items.FirstOrDefault();
        //    return new Livestream()
        //    {
        //        VideoId = videoId
        //    };
        //}
    }
}

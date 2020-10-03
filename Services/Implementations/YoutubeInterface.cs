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
    [Serializable]
    public class StartCapturingTooSoonException : Exception
    {
        public StartCapturingTooSoonException() { }
        public StartCapturingTooSoonException(string message) : base(message) { }
        public StartCapturingTooSoonException(string message, Exception inner) : base(message, inner) { }
        protected StartCapturingTooSoonException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class NoActualEndtimeException : Exception
    {
        public NoActualEndtimeException() { }
        public NoActualEndtimeException(string message) : base(message) { }
        public NoActualEndtimeException(string message, Exception inner) : base(message, inner) { }
        protected NoActualEndtimeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

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
            YouTubeService t = GetYoutubeService();

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

        internal async Task<string> GetVideoId(string videoId)
        {
            try
            {
                var uri = new Uri(videoId);
                var querryDict = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                videoId = querryDict["v"];
            }
            catch
            {
            }

            YouTubeService t = GetYoutubeService();
            VideosResource.ListRequest request = t.Videos.List("id");
            request.Id = videoId;
            VideoListResponse response = await request.ExecuteAsync();
            if (response.Items.Count > 0)
            {
                return videoId;
            }
            else
            {
                return null;
            }
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

        internal async Task<VideoDto> GetVideoInfo(YouTubeService ytService, string videoId, bool getScheduledTime = false, bool getActualEndTime = false)
        {
            try
            {
                VideosResource.ListRequest request = ytService.Videos.List("snippet,id,liveStreamingDetails,statistics");
                request.Id = videoId;
                VideoListResponse result = await request.ExecuteAsync();
                Video livestream = result.Items.FirstOrDefault();

                DateTime startTime = default;
                DateTime endTime = default;

                if (getScheduledTime)
                {
                    DateTime.TryParse(livestream.LiveStreamingDetails.ScheduledStartTime, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out startTime);
                }
                else
                if (!DateTime.TryParse(livestream.LiveStreamingDetails.ActualStartTime, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out startTime))
                {
                    throw new StartCapturingTooSoonException(videoId);
                }

                if (getActualEndTime)
                {
                    if (!DateTime.TryParse(livestream.LiveStreamingDetails.ActualEndTime, null, styles: DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out endTime))
                    {
                        throw new NoActualEndtimeException(videoId);
                    }
                }

                return new VideoDto()
                {
                    VideoId = livestream?.Id,
                    StartTime = startTime,
                    EndTime = endTime
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(GetVideoInfo));
                throw;
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

using discordbot.DAL.Infrastructure.Interfaces;
using discordbot.Services.DTOs;

using Newtonsoft.Json;

using System;

namespace discordbot.DAL.Entities
{
    public class Video : TValue
    {
        public Video() { }
        public Video(VideoDto video)
        {
            this.VideoId = video.VideoId;
            this.VideoTitle = video.VideoTitle;
            this.Misc = video.Misc;
            this.StartTime = video.StartTime;
            this.EndTime = video.EndTime;
        }

        [JsonIgnore]
        public string Key => VideoId;

        [JsonIgnore]
        public object Value => this;

        public string VideoId { get; set; }
        public string VideoTitle { get; set; }
        public string Misc { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

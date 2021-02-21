using discordbot.DAL.Entities;

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace discordbot.Services.DTOs
{
    public class VideoDto
    {
        public VideoDto() { }
        internal VideoDto(Video vd)
        {
            VideoId = vd.VideoId;
            VideoTitle = vd.VideoTitle;
            Misc = vd.Misc;
            StartTime = vd.StartTime;
            EndTime = vd.EndTime;
        }

        public string VideoId { get; set; }

        public string VideoTitle { get; set; }

        [JsonIgnore]
        public string Misc { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsLive => DateTime.UtcNow > StartTime && DateTime.UtcNow < EndTime;

        public IEnumerable<TimeStampDto> TimeStamps { get; set; } = null;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Utility.GetYoutubeUrl(VideoId));
            sb.AppendLine("Time is expressed in UTC format");
            sb.AppendFormat("Start time: {0}", StartTime.ToLongTimeString());
            sb.AppendLine();
            sb.AppendFormat("End time: {0}", EndTime.ToLongTimeString());
            return sb.ToString();
        }
    }
}

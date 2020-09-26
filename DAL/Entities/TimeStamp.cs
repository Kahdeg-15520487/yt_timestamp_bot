using discordbot.Services.DTOs;

using LiteDB;

using System;
using System.Collections.Generic;
using System.Text;

namespace discordbot.DAL.Entities
{
    class TimeStamp : BaseEntity
    {
        public DateTime ActualTime { get; set; }
        public DateTime LastModified { get; set; }
        public double Time { get; set; }
        public string TagContent { get; set; }
        public string VideoId { get; set; }
        public ulong UserId { get; set; }
        public string UserName { get; internal set; }

        public TimeStamp() { }

        public TimeStamp(TimeStampDto dto)
        {
            TagContent = dto.TagContent;
            VideoId = dto.VideoId;
            UserId = dto.UserId;
            ActualTime = dto.ActualTime;
            LastModified = dto.LastModified;
            Time = dto.Time;
            UserName = dto.UserName;
        }

        public TimeStamp(string tagContent, string videoId, DateTime videoStartTime, ulong userId, string userName, double min)
        {
            this.TagContent = tagContent;
            this.VideoId = videoId;
            this.UserId = userId;
            this.UserName = userName;
            ActualTime = DateTime.UtcNow - TimeSpan.FromMinutes(min);
            LastModified = DateTime.UtcNow;
            Time = (ActualTime - videoStartTime).TotalSeconds;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} : {1} at {2}", Time, TagContent, Utility.GetYoutubeUrl(VideoId));
            return sb.ToString();
        }
    }
}

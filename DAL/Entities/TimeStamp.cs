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
        public double Time { get; set; }
        public string TagContent { get; set; }
        public string VideoId { get; set; }
        public ulong UserId { get; set; }
        public string UserName { get; internal set; }

        public TimeStamp() { }

        public TimeStamp(TimeStampDto tag)
        {
            TagContent = tag.TagContent;
            VideoId = tag.VideoId;
            UserId = tag.UserId;
            ActualTime = tag.ActualTime;
            Time = tag.Time;
            UserName = tag.UserName;
        }

        public TimeStamp(string tagContent, string videoId, DateTime videoStartTime, ulong userId, string userName)
        {
            this.TagContent = tagContent;
            this.VideoId = videoId;
            this.UserId = userId;
            this.UserName = userName;
            ActualTime = DateTime.UtcNow;
            Time = (DateTime.UtcNow - videoStartTime).TotalSeconds;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} : {1} at {2}", Time, TagContent, Utility.GetYoutubeUrl(VideoId));
            return sb.ToString();
        }
    }
}

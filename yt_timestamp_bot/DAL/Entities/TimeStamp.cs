using discordbot.DAL.Infrastructure.Interfaces;
using discordbot.Services.DTOs;

using Newtonsoft.Json;

using System;
using System.Text;

namespace discordbot.DAL.Entities
{
    class TimeStamp : TValue
    {

        [JsonIgnore]
        public string Key => Id.ToString();

        [JsonIgnore]
        public object Value => this;

        public Guid Id { get; set; }
        public DateTime ActualTime { get; set; }
        public DateTime LastModified { get; set; }
        public double Time { get; set; }
        public string TagContent { get; set; }
        public string VideoId { get; set; }
        public ulong UserId { get; set; }
        public string UserName { get; set; }
        public ulong MessageId { get; set; }

        public TimeStamp() { }

        public TimeStamp(TimeStampDto dto)
        {
            Id = Guid.NewGuid();
            TagContent = dto.TagContent;
            VideoId = dto.VideoId;
            UserId = dto.UserId;
            ActualTime = dto.ActualTime;
            LastModified = dto.LastModified;
            Time = dto.Time;
            UserName = dto.UserName;
        }

        public TimeStamp(string tagContent, string videoId, DateTime videoStartTime, int second, ulong userId, string userName, ulong messageId)
        {
            Id = Guid.NewGuid();
            this.TagContent = tagContent;
            this.VideoId = videoId;
            this.UserId = userId;
            this.UserName = userName;
            this.MessageId = messageId;
            ActualTime = DateTime.UtcNow - TimeSpan.FromSeconds(second);
            LastModified = DateTime.UtcNow;
            Time = (ActualTime - videoStartTime).TotalSeconds;
        }

        public TimeStamp(string tagContent, string videoId, DateTime videoStartTime, DateTime actualTime, ulong userId, string userName, ulong messageId)
        {
            this.TagContent = tagContent;
            this.VideoId = videoId;
            this.UserId = userId;
            this.UserName = userName;
            this.MessageId = messageId;
            ActualTime = actualTime;
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

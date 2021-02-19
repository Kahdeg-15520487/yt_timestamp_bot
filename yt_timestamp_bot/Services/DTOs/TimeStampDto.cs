using discordbot.DAL.Entities;

using System;
using System.Collections.Generic;
using System.Text;

namespace discordbot.Services.DTOs
{
    public class TimeStampDto
    {
        public string Id { get; set; }
        public string TagContent { get; set; }
        public DateTime ActualTime { get; set; }
        public DateTime LastModified { get; set; }
        public double Time { get; set; }
        public string VideoId { get; set; }
        public ulong UserId { get; private set; }
        public string UserName { get; private set; }

        public TimeStampDto() { }

        internal TimeStampDto(TimeStamp timestamp)
        {
            Id = timestamp.Id.ToString();
            TagContent = timestamp.TagContent;
            ActualTime = timestamp.ActualTime;
            LastModified = timestamp.LastModified;
            VideoId = timestamp.VideoId;
            Time = timestamp.Time;
            UserId = timestamp.UserId;
            UserName = timestamp.UserName;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t:{1}", Utility.ToTimeStamp(Time), TagContent);
            return sb.ToString();
        }
    }
}

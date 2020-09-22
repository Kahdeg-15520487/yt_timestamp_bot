using discordbot.DAL.Entities;

using System;
using System.Collections.Generic;
using System.Text;

namespace discordbot.Services.DTOs
{
    public class Tag
    {
        public string TagContent { get; set; }
        public DateTime ActualTime { get; set; }
        public double Time { get; set; }
        public string VideoId { get; set; }
        public ulong UserId { get; private set; }
        public string UserName { get; private set; }

        public Tag() { }

        internal Tag(TimeStamp timestamp)
        {
            TagContent = timestamp.TagContent;
            ActualTime = timestamp.ActualTime;
            VideoId = timestamp.VideoId;
            Time = timestamp.Time;
            UserId = timestamp.UserId;
            UserName = timestamp.UserName;
        }

        internal static Tag Create(string tagContent, string videoId, DateTime videoStartTime, ulong userId, string userName)
        {
            return new Tag()
            {
                TagContent = tagContent,
                ActualTime = DateTime.UtcNow,
                Time = (DateTime.UtcNow - videoStartTime).TotalSeconds,
                VideoId = videoId,
                UserId = userId,
                UserName = userName
            };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}\t:{1}", Utility.ToTimeStamp(Time), TagContent);
            return sb.ToString();
        }
    }
}

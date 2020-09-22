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

        public TimeStamp(Tag tag)
        {
            TagContent = tag.TagContent;
            VideoId = tag.VideoId;
            UserId = tag.UserId;
            ActualTime = tag.ActualTime;
            Time = tag.Time;
            UserName = tag.UserName;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} : {1} at https://youtube.com/watch?v={2}", Time, TagContent, VideoId);
            return sb.ToString();
        }
    }
}

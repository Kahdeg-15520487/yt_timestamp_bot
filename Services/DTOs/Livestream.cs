using System;
using System.Collections.Generic;
using System.Text;

namespace discordbot.Services.DTOs
{
    public class Livestream
    {
        public string VideoId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsLive => DateTime.UtcNow > StartTime && DateTime.UtcNow < EndTime;

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

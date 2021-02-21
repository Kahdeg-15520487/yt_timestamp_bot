using discordbot.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.DAL.Entities
{
    class Video
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
        public string VideoId { get; set; }
        public string VideoTitle { get; set; }
        public string Misc { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

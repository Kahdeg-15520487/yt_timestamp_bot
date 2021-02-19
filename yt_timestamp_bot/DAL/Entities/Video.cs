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
            this.StartTime = video.StartTime;
            this.EndTime = video.EndTime;
        }
        public string VideoId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}

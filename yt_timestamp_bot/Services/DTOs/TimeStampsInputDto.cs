using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.Services.DTOs
{
    public class TimeStampsInputDto
    {
        public string Id { get; set; }
        public string TagContent { get; set; }
        public DateTime ActualTime { get; set; }
        public DateTime LastModified { get; set; }
        public double Time { get; set; }
        public string VideoId { get; set; }
        public ulong UserId { get; private set; }
        public string UserName { get; private set; }
    }
}

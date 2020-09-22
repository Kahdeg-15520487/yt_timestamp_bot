using discordbot.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.Services.Interfaces
{
    public interface IVideoService
    {
        VideoDto GetVideo(string videoId);
        IEnumerable<TimeStampDto> GetTimeStamps(string videoId);
    }
}

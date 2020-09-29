using discordbot.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace discordbot.Services.Interfaces
{
    public interface ITagService
    {
        Task<bool> StartTag(string videoId = null);
        void EndTag();
        TimeStampDto GetTag(string tag);
        IEnumerable<TimeStampDto> ListTag(string videoId = null);
        TimeStampDto AddTag(string tag, ulong userId, string userName, double min = 0);
        TimeStampDto AddTag(string tag, ulong userId, string userName, DateTime actualTime);
        TimeStampDto EditTag(ulong userId, string tag);
        bool DeleteTag(string tag);

        VideoDto CurrentLiveStream { get; }
        bool IsLive { get; }
        bool ShiftTag(int x, string videoId = null);
    }
}

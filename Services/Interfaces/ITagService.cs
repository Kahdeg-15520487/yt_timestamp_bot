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
        Task EndTag();
        TimeStampDto GetTag(string tag);
        List<TimeStampDto> ListTag(string videoId = null);
        List<TimeStampDto> ListTag(string videoId, ulong userId);
        TimeStampDto AddTag(string tag, ulong userId, string userName, int second = 0);
        TimeStampDto AddTag(string tag, ulong userId, string userName, DateTime actualTime);
        TimeStampDto EditTag(ulong userId, string tag);
        bool DeleteTag(string tag);

        VideoDto CurrentLiveStream { get; }
        bool IsLive { get; }
        bool ShiftTag(int x, string videoId = null);
    }
}

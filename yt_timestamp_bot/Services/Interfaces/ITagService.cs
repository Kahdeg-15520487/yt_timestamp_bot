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
        TimeStampDto AddTag(string tag, ulong userId, string userName, ulong messageId, int second = 0);
        TimeStampDto AddTag(string tag, ulong userId, string userName, ulong messageId, DateTime actualTime);
        TimeStampDto EditTag(ulong userId, string tag);
        TimeStampDto EditTag(ulong userId, ulong messageId, string tag);
        bool DeleteTag(string tag);

        VideoDto CurrentLiveStream { get; }
        bool IsLive { get; }
        bool ShiftTag(int x, string videoId = null);
    }
}

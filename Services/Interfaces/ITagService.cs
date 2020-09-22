using discordbot.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace discordbot.Services.Interfaces
{
    public interface ITagService
    {
        Task<bool> StartTag();
        void EndTag();
        TimeStampDto GetTag(string tag);
        IEnumerable<TimeStampDto> ListTag(string videoId = null);
        TimeStampDto AddTag(string tag, ulong userId, string userName);
        TimeStampDto EditTag(ulong userId, string tag);
        bool DeleteTag(string tag);

        VideoDto CurrentLiveStream { get; }
        bool IsLive { get; }

        DateTime RecalculateTag(DateTime dt, string videoId = null);
    }
}

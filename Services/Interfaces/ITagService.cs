using discordbot.Services.DTOs;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace discordbot.Services.Interfaces
{
    public interface ITagService
    {
        Task StartTag();
        void EndTag();
        Tag GetTag(string tag);
        IEnumerable<Tag> ListTag(string videoId = null);
        Tag AddTag(string tag, ulong userId, string userName);
        Tag EditTag(ulong userId, string tag);
        bool DeleteTag(string tag);

        Livestream CurrentLiveStream { get; }

        DateTime RecalculateTag(DateTime dt, string videoId = null);
    }
}

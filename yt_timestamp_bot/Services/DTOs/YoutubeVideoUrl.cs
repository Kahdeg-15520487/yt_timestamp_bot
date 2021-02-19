using Discord.Commands;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.Services.DTOs
{
    public class YoutubeVideoUrl
    {
        public YoutubeVideoUrl(string videoId)
        {
            this.VideoId = videoId;
        }

        public string VideoId { get; set; }
        public override string ToString()
        {
            return Utility.GetYoutubeUrl(VideoId);
        }
    }

    internal class YoutubeVideoUrlTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                Uri uri = new Uri(input);
                Dictionary<string, StringValues> querryDict = QueryHelpers.ParseQuery(uri.Query);
                string videoId = querryDict["v"];

                YoutubeVideoUrl result = new YoutubeVideoUrl(videoId);
                return Task.FromResult(TypeReaderResult.FromSuccess(result));
            }
            catch (UriFormatException uex)
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(new YoutubeVideoUrl(input)));
            }
            catch
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed a YoutubeVideoUrl"));
            }

        }
    }
}

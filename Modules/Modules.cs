using Discord;
using Discord.Commands;
using Discord.WebSocket;

using discordbot;
using discordbot.Services;
using discordbot.Services.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    [Name(DiscordModule.MODULE_NAME)]
    [Summary(DiscordModule.MODULE_DESCRIPTION)]
    public class DiscordModule : ModuleBase<SocketCommandContext>
    {
        public const string MODULE_NAME = "tag";
        public const string MODULE_DESCRIPTION = "tag timestamp of a livestream video";

        private readonly ILogger<DiscordModule> logger;
        private readonly YoutubeInterface ytService;
        private readonly ITagService tagService;

        public static string HELP_STRING { get; internal set; } = string.Empty;

        internal DiscordModule(ILogger<DiscordModule> logger,
            YoutubeInterface ytService,
            ITagService tagService)
        {
            this.logger = logger;
            this.ytService = ytService;
            this.tagService = tagService;
            logger.LogInformation("Loaded default Module");
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            base.BeforeExecute(command);
        }

        [Command("help")]
        [Summary("Get help")]
        public Task PrintHelpAsync()
        {
            return base.ReplyAsync(DiscordModule.HELP_STRING);
        }

        [Command("start")]
        [Summary("start recording tag")]
        public async Task StartTag()
        {
            await tagService.StartTag();
        }

        [Command("t")]
        [Summary("tag current time in livestream")]
        public async Task Tag([Summary("tag")][Remainder] string tag)
        {
            tagService.AddTag(tag, Context.User.Id, Context.User.Username);
            logger.LogInformation($"{Context.User.Id}|{Context.User.Username} tagged {tag}");
            //discordbot.Services.DTOs.Livestream tt = await ytService.GetStartTime(videoId);
            //await base.ReplyAsync(tt.ToString());
        }

        [Command("r")]
        [Summary("Recalculate time stamp with a new time(in UTC)\n\t\t\tex: r hh:mm:ss")]
        public async Task CallibrateTag([Summary("new time")] string newTime)
        {
            DateTime dt = DateTime.Parse(newTime);
            DateTime oldDt = tagService.RecalculateTag(dt);
            await base.ReplyAsync($"recalculate time stamp from {oldDt.ToLongTimeString()} to {dt.ToLongTimeString()}");
        }

        [Command("r")]
        [Summary("Recalculate time stamp with a new time(in UTC) with videoId\n\t\t\tex: r hh:mm:ss videoId")]
        public async Task CallibrateTag([Summary("new time")] string newTime, [Summary("videoId")] string videoId)
        {
            DateTime dt = DateTime.Parse(newTime);
            DateTime oldDt = tagService.RecalculateTag(dt, videoId);
            await base.ReplyAsync($"recalculate time stamp from {oldDt.ToLongTimeString()} to {dt.ToLongTimeString()}");
        }

        [Command("list")]
        [Summary("list tag")]
        public async Task ListTag()
        {
            if (tagService.CurrentLiveStream == null)
            {
                return;
            }
            List<string> tagSegments = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (var t in tagService.ListTag())
            {
                sb.AppendFormat("[{0}]({1}) : {2} by {3}", Utility.ToTimeStamp(t.Time), Utility.GetYoutubeUrlWithTime(t.VideoId, t.Time), t.TagContent, t.UserName);
                sb.AppendLine();

                if (sb.Length >= 800)
                {
                    tagSegments.Add(sb.ToString());
                    sb.Clear();
                }
            }

            if (sb.Length >= 0)
            {
                tagSegments.Add(sb.ToString());
            }

            bool isFirst = true;
            foreach (var segment in tagSegments)
            {
                var embed = new EmbedBuilder();

                if (isFirst)
                {
                    isFirst = false;
                    embed.Title = "Tags list";
                    embed.Description = Utility.GetYoutubeUrl(tagService.CurrentLiveStream.VideoId);
                }

                embed.AddField("tags:", segment)
                     .WithCurrentTimestamp();
                await base.ReplyAsync(embed: embed.Build());
            }
        }

        [Command("list")]
        [Summary("list tag with videoId\n\t\t\tex: list VQu7r649K0s")]
        public async Task ListTag([Summary("videoId")] string videoId)
        {
            List<string> tagSegments = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (var t in tagService.ListTag(videoId))
            {
                sb.AppendFormat("[{0}]({1}) : {2} by {3}", Utility.ToTimeStamp(t.Time), Utility.GetYoutubeUrlWithTime(t.VideoId, t.Time), t.TagContent, t.UserName);
                sb.AppendLine();

                if (sb.Length >= 800)
                {
                    tagSegments.Add(sb.ToString());
                    sb.Clear();
                }
            }

            if (sb.Length >= 0)
            {
                tagSegments.Add(sb.ToString());
            }

            bool isFirst = true;
            foreach (var segment in tagSegments)
            {
                var embed = new EmbedBuilder();

                if (isFirst)
                {
                    isFirst = false;
                    embed.Title = "Tags list";
                    embed.Description = Utility.GetYoutubeUrl(videoId);
                }

                embed.AddField("tags:", segment)
                     .WithCurrentTimestamp();
                await base.ReplyAsync(embed: embed.Build());
            }
        }

        [Command("e")]
        [Summary("edit previous tag")]
        public async Task EditTag([Summary("tag")][Remainder] string tag)
        {
            var oldTag = tagService.EditTag(Context.User.Id, tag);
            logger.LogInformation($"{Context.User.Id}|{Context.User.Username} edited \"{oldTag.TagContent}\" to \"{tag}\"");
        }

        private SocketUser GetUserFromContext()
        {
            return this.Context.User;
        }
    }
}

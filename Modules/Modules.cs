using Discord;
using Discord.Commands;
using Discord.WebSocket;

using discordbot;
using discordbot.Modules;
using discordbot.Services;
using discordbot.Services.DTOs;
using discordbot.Services.Interfaces;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace discordbot.Modules
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
        public async Task PrintHelpAsync()
        {
            await base.ReplyAsync(DiscordModule.HELP_STRING);
        }

        [Command("start")]
        [Summary("start recording tag")]
        public async Task StartTag()
        {
            if (!await tagService.StartTag())
            {
                await base.ReplyAsync("Found no stream");
            }
        }

        [Command("start")]
        [Summary("start recording tag with videoId\n\t\t\tex: start pG-KF1HFt4w\n\t\t\t    start https://www.youtube.com/watch?v=pG-KF1HFt4w")]
        public async Task StartTag([Summary("videoId")] string videoId)
        {
            if (await tagService.StartTag(videoId))
            {
                await base.ReplyAsync($"Start recording tag for {videoId}");
            }
            else
            {
                await base.ReplyAsync("Found no stream");
            }
        }

        [Command("t")]
        [Summary("tag current time in livestream")]
        public async Task Tag([Summary("tag")][Remainder] string tag)
        {
            tagService.AddTag(tag, Context.User.Id, Context.User.Username);
            logger.LogInformation($"{Context.User.Id}|{Context.User.Username} tagged {tag}");
        }

        [Command("ct")]
        [Summary("tag in the past in livestream\n\t\t\tex: ct 0.5 words")]
        public async Task Tag([Summary("time to subtract in minutes")] double min, [Summary("tag")][Remainder] string tag)
        {
            tagService.AddTag(tag, Context.User.Id, Context.User.Username, min);
            logger.LogInformation($"{Context.User.Id}|{Context.User.Username} tagged {tag} with backtrack of {min} minutes");
        }

        [Command("backward")]
        [Summary("Shift time stamp with backward x seconds\n\t\t\tex: backward 90")]
        [RequireRole(nameof(Roles.TagCalibrator))]
        public async Task ShiftTagBackward([Summary("time to shift")] int x, [Summary("videoId")] string videoId)
        {
            if (!tagService.IsLive)
            {
                return;
            }
            await this.ShiftTagForward(-x, tagService.CurrentLiveStream.VideoId);
        }

        [Command("forward")]
        [Summary("Recalculate time stamp with a new time(in UTC) with videoId\n\t\t\tex: r hh:mm:ss videoId")]
        [RequireRole(nameof(Roles.TagCalibrator))]
        public async Task ShiftTagForward([Summary("time to shift")] int x, [Summary("videoId")] string videoId)
        {
            try
            {
                tagService.ShiftTag(x, videoId);
                await ReplyAsync($"Shifted {x} seconds {(x > 0 ? "forward" : "backward")} for video {Utility.GetYoutubeUrl(videoId)}");
            }
            catch (KeyNotFoundException knfex)
            {
                await ReplyAsync($"Video not found");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Can't shift tag time for {videoId}");
            }
        }

        [Command("list")]
        [Summary("list tag")]
        public async Task ListTag()
        {
            if (tagService.CurrentLiveStream == null)
            {
                return;
            }

            try
            {
                await this.ListTag(tagService.CurrentLiveStream.VideoId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, nameof(ListTag));
                await this.ReplyAsync($"Error when listing tags for {tagService.CurrentLiveStream.VideoId}");
            }
        }

        [Command("list")]
        [Summary("list tag with videoId\n\t\t\tex: list VQu7r649K0s")]
        public async Task ListTag([Summary("videoId")] string videoId)
        {
            List<string> tagSegments = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach (TimeStampDto t in tagService.ListTag(videoId))
            {
                sb.AppendFormat("[{0}]({1}) : {2} by {3}", Utility.ToTimeStamp(t.Time), Utility.GetYoutubeUrlWithTime(t.VideoId, t.Time), t.TagContent, t.UserName);
                sb.AppendLine();

                if (sb.Length >= 800)
                {
                    tagSegments.Add(sb.ToString());
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
            {
                tagSegments.Add(sb.ToString());
            }
            else
            {
                await this.ReplyAsync($"No tags is registered for {videoId}");
                return;
            }

            bool isFirst = true;
            foreach (string segment in tagSegments)
            {
                EmbedBuilder embed = new EmbedBuilder();

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
            TimeStampDto oldTag = tagService.EditTag(Context.User.Id, tag);
            logger.LogInformation($"{Context.User.Id}|{Context.User.Username} edited \"{oldTag.TagContent}\" to \"{tag}\"");
        }

        private SocketUser GetUserFromContext()
        {
            return this.Context.User;
        }
    }
}

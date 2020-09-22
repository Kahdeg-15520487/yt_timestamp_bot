using Discord.Commands;
using Discord.WebSocket;

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
        public const string MODULE_NAME = "discore_module";
        public const string MODULE_DESCRIPTION = "example module";

        private readonly ILogger<DiscordModule> logger;

        public static string HELP_STRING { get; internal set; } = string.Empty;

        public DiscordModule(ILogger<DiscordModule> logger)
        {
            this.logger = logger;
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

        private SocketUser GetUserFromContext()
        {
            return this.Context.User;
        }
    }
}

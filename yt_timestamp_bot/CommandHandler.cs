using Discord;
using Discord.Commands;
using Discord.WebSocket;

using discordbot.Modules;
using discordbot.Services.DTOs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace discordbot
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider serviceProvider;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<CommandHandler> logger;
        private readonly string commandPrefix;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _commands = commands;
            _client = client;
            this.serviceProvider = serviceProvider;
            logger = this.serviceProvider.GetService(typeof(ILogger<CommandHandler>)) as ILogger<CommandHandler>;
            commandPrefix = configuration["_PREFIX"];
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _client.MessageUpdated += HandleUpdatedCommandAsync;
            //_client.ReactionAdded += HandleReactionAddedCommandAsync;

            _commands.AddTypeReader<YoutubeVideoUrl>(new YoutubeVideoUrlTypeReader());

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            IEnumerable<ModuleInfo> modules = await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: this.serviceProvider);

            StringBuilder sb = new StringBuilder();
            StringBuilder help = new StringBuilder();
            sb.AppendLine(Program.APPLICATION_NAME);
            sb.AppendLine(Program.VERSION);
            sb.AppendLine("Discord Modules loaded:");
            help.AppendLine(Program.APPLICATION_NAME);
            help.AppendLine(Program.VERSION);
            foreach (ModuleInfo module in modules)
            {
                sb.AppendLine(module.Name);
                sb.AppendLine(module.Summary);
                sb.AppendLine("Commands: ");
                foreach (CommandInfo cmd in module.Commands)
                {
                    sb.AppendLine("    " + cmd.Name);
                    sb.AppendLine("    " + cmd.Summary);
                }
                sb.AppendLine("-----");

                //only print description from allowed module
                //in case you have some kind of hidden module like administration or debug
                if (module.Name == DiscordModule.MODULE_NAME)
                {
                    help.AppendLine("```");
                    help.AppendLine(module.Name);
                    help.AppendLine(module.Summary);
                    help.AppendLine("Commands: ");
                    foreach (CommandInfo cmd in module.Commands)
                    {
                        help.AppendLine("    " + cmd.Name + '\t' + cmd.Summary);
                    }
                    help.AppendLine("```");
                }
                DiscordModule.HELP_STRING = help.ToString();
            }
            logger.LogInformation(sb.ToString());

            var changeLogsRaw = File.ReadAllText("changelog.json");
        }

        //private async Task HandleReactionAddedCommandAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        //{
        //    // Don't process the command if it was a system message
        //    SocketUserMessage message = messageParam as SocketUserMessage;
        //    if (message == null) return;

        //    // Create a number to track where the prefix ends and the command begins
        //    int argPos = 0;

        //    // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        //    if (!(message.HasStringPrefix(commandPrefix, ref argPos) ||
        //        message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
        //        message.Author.IsBot)
        //        return;

        //    // Create a WebSocket-based command context based on the message
        //    SocketCommandContext context = new SocketCommandContext(_client, message);
        //    logger.LogInformation($"{message.Author.Id}|{message.Author.Username}: updated |{oldMessage.Value}| to |{message.Content}|");
        //    // Execute the command with the command context we just
        //    // created, along with the service provider for precondition checks.
        //    await _commands.ExecuteAsync(
        //        context: context,
        //        argPos: argPos,
        //        services: this.serviceProvider);
        //}

        private async Task HandleUpdatedCommandAsync(Cacheable<IMessage, ulong> oldMessage, SocketMessage messageParam, ISocketMessageChannel channel)
        {
            // Don't process the command if it was a system message
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix(commandPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            SocketCommandContext context = new SocketCommandContext(_client, message);
            logger.LogInformation($"{message.Author.Id}|{message.Author.Username}: updated |{oldMessage.Value}| to |{message.Content}|");
            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: this.serviceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix(commandPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            SocketCommandContext context = new SocketCommandContext(_client, message);
            logger.LogInformation($"{message.Author.Id}|{message.Author.Username}: {message.Content}");
            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: this.serviceProvider);
        }
    }
}

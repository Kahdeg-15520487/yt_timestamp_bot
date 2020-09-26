using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace discordbot.BackgroundServices
{
    class DiscordHandlerHostedService : BackgroundService
    {
        private readonly IServiceProvider services;
        private readonly IConfiguration configuration;
        private readonly ILogger<DiscordHandlerHostedService> logger;

        public DiscordHandlerHostedService(IServiceProvider services, IConfiguration configuration, ILogger<DiscordHandlerHostedService> logger)
        {
            this.services = services;
            this.configuration = configuration;
            this.logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += Log;
            services.GetRequiredService<CommandService>().Log += Log;

            string token = configuration["_BOTTOKEN"];

            try
            {
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Exception when connecting with Discord");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
                logger.LogError(sb.ToString());
            }

            try
            {
                await services.GetRequiredService<CommandHandler>().InstallCommandsAsync();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Exception when loading command modules");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
                logger.LogError(sb.ToString());
            }

            //infinite wait
            await Task.Delay(-1);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        private Task Log(LogMessage msg)
        {
            logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        }
    }
}

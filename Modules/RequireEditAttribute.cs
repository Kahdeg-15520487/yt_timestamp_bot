using Discord.Commands;
using Discord.WebSocket;

using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace discordbot.Modules
{
    public class NoEditAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var loggerFactory = services.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<NoEditAttribute>();
            if (context.Message.EditedTimestamp == null)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                logger.LogInformation($"{{{context.User.Username}:{context.User.Id}}} {command.Name} doesn't support message edit");
                return Task.FromResult(PreconditionResult.FromError("Command doesnt support message edit"));
            }
        }
    }
}

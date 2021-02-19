using Discord.Commands;
using Discord.WebSocket;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace discordbot.Modules
{
    public class NoEditAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Message.EditedTimestamp == null)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Command doesnt support message edit"));
            }
        }
    }
}

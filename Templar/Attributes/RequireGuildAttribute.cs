using System;
using System.Threading.Tasks;

using Discord.Commands;

namespace Templar
{
    public class RequireGuildAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild is null)
            {
                return PreconditionResult.FromError("Must be run in guild.");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord.Commands;

namespace Templar.Attributes
{
    public class RequireNotDebugAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Debugger.IsAttached ? Task.FromResult(PreconditionResult.FromError("This command is only available in the production bot.")) :
                Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}

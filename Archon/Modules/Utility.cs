using System.Threading.Tasks;

using Discord.Commands;

namespace Archon.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }
    }
}

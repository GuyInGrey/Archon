using System;
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

        [Command("pow")]
        public async Task Say(int a, int b)
        {
            await ReplyAsync(Math.Pow(a, b).ToString());
        }
    }
}

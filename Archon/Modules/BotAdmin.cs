using System;
using System.Threading.Tasks;

using Discord.Commands;

namespace Archon
{
    public class BotAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("testerror")]
        public async Task TestError()
        {
            throw new Exception("Test error!");
        }
    }
}

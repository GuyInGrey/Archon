using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Archon.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            var ping = Context.Client.Latency;

            var e = new EmbedBuilder()
                .WithTitle("Pong!")
                .WithDescription(ping + "ms")
                .WithColor(ping < 200 ? Color.Green : ping < 500 ? Color.LightOrange : Color.Red)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: e.Build());
        }

        [Command("pow")]
        public async Task Say(int a, int b)
        {
            await ReplyAsync(Math.Pow(a, b).ToString());
        }
    }
}

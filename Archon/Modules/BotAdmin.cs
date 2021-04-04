using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord.Commands;

using Templar;

namespace Archon
{
    public partial class BotAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("testerror")]
        [RequireOwner]
        public async Task TestError()
        {
            throw new Exception("Test error!");
        }

        [Command("restart")]
        [RequireOwner]
        public async Task Restart()
        {
            await Context.ReactOk();
            await Context.Client.StopAsync();
            if (Debugger.IsAttached) { return; }

            var p = new ProcessStartInfo("bash")
            {
                Arguments = "-c ./runBot.sh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process.Start(p);
            Process.GetCurrentProcess().Kill();
        }

        [Command("update")]
        [RequireOwner]
        public async Task Update()
        {
            await Context.ReactOk();
            await Context.Client.StopAsync();
            if (Debugger.IsAttached) { return; }

            var p = new ProcessStartInfo("bash")
            {
                Arguments = "-c ./updateBot.sh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            Process.Start(p);
            Process.GetCurrentProcess().Kill();
        }
    }
}

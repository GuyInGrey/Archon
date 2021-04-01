using System;
using System.Threading.Tasks;

using Discord.Commands;

using Templar.Database;

namespace Archon
{
    public class PartyCmd : ModuleBase<SocketCommandContext>
    {
        private static Database<Party> Database = new("parties");

        [Command("add")]
        public async Task Add()
        {
            var m = new Party()
            {
                Host = 15,
                Guests = "",
                CreationTime = DateTime.Now,
                TextChannel = 0,
                VoiceChannel = 1,
            };

            await Database.Insert(m);
        }

        [Command("count")]
        public async Task Count()
        {
            var count = (await Database.GetByProperty("Host", 15)).Count;
            await ReplyAsync(count.ToString());
        }
    }
}

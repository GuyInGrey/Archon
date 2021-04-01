using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Templar;
using Templar.Database;

namespace Archon
{
    public class UtilityCmd : ModuleBase<SocketCommandContext>
    {
        static Database<Tag> Database = new ("tags");

        [Command("tag")]
        public async Task Tag(string name, [Remainder]string con = "")
        {
            await Database.DeleteByProperty("Name", name);

            if (!(con == "" && Context.Message.Attachments.Count == 0))
            {
                var t = new Tag()
                {
                    Name = name,
                    Content = con,
                };
                t.SetAttachments(Context.Message.Attachments.Select(a => a.Url).ToArray());

                await Database.Insert(t);
            }
            await Context.ReactOk();
        }

        public static async Task<bool> RunTag(SocketMessage msg, string prefix)
        {


            var argPos = 0;
            var hasPrefix = msg.HasStringPrefix(prefix, ref argPos));
            if (!(hasPrefix) || msg.Author.IsBot) { return false; }

            var remainder = msg.Content.SplitAt(argPos).right;
            (var commandName, var suffix) = remainder.SplitAt(remainder.IndexOf(' '));

            var tags = Database.GetByProperty("Name", );
        }
    }
}

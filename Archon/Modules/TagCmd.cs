using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Templar;
using Templar.Database;

namespace Archon
{
    public class TagCmd : ModuleBase<SocketCommandContext>
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

        public static async Task<bool> RunTag(SocketMessage arg, string prefix)
        {
            if (arg is not SocketUserMessage msg) { return false; }

            var argPos = 0;
            var hasPrefix = msg.HasStringPrefix(prefix, ref argPos);
            if (!(hasPrefix) || msg.Author.IsBot) { return false; }

            var remainder = msg.Content.SplitAt(argPos).right;
            (var commandName, var suffix) = remainder.SplitAt(remainder.IndexOf(' '));

            var tags = await Database.GetByProperty("Name", commandName);
            if (tags.Count <= 0) { return false; }
            var tag = tags[0];

            await PostTag(tag, msg.Channel, suffix, msg.Author);

            return true;
        }

        public static async Task PostTag(Tag tag, ISocketMessageChannel channel, string suffix, SocketUser author)
        {
            if (tag.Content != string.Empty)
            {
                await channel.SendMessageAsync(tag.Content.Replace("{author}", author.Mention).Replace("{suffix}", suffix));
            }
            foreach (var a in tag.GetAttachments())
            {
                await channel.SendMessageAsync(a);
            }
        }
    }
}

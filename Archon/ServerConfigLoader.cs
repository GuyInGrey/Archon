using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json.Linq;

using Templar;
using Templar.Database;

namespace Archon
{
    [Group("config")]
    [RequireGuild]
    public class ServerConfigLoader : ModuleBase<SocketCommandContext>
    {
        static Database<ServerConfig> Database = new("serverconfigs");

        public static async Task<JObject> Get(ulong serverID)
        {
            var matches = await Database.GetByProperty("ServerID", serverID);
            if (matches.Count <= 0)
            {
                return new JObject();
            }

            return matches[0].GetConfigObject();
        }

        public static async Task Set(ulong serverID, JObject config)
        {
            await Database.DeleteByProperty("ServerID", serverID);
            if (config is null) { return; }
            var obj = new ServerConfig()
            {
                ServerID = serverID,
            };
            obj.SetConfigObject(config);
            await Database.Insert(obj);
        }

        public static async Task Set(ulong serverID, string propName, object val)
        {
            var config = await Get(serverID);
            if (config.ContainsKey(propName))
            {
                config.Remove(propName);
            }
            config.Add(propName, JToken.FromObject(val));
            await Set(serverID, config);
        }

        [Command("set")]
        public async Task Set(string name, string value)
        {
            await Set(Context.Guild.Id, name, value);
            await Context.ReactOk();
        }

        [Command("set")]
        public async Task Set(string name, ulong value)
        {
            await Set(Context.Guild.Id, name, value);
            await Context.ReactOk();
        }

        [Command("set")]
        public async Task Set(string name, SocketGuildUser value)
        {
            await Set(Context.Guild.Id, name, value.Id);
            await Context.ReactOk();
        }

        [Command("set")]
        public async Task Set(string name, SocketGuildChannel value)
        {
            await Set(Context.Guild.Id, name, value.Id);
            await Context.ReactOk();
        }

        [Command("unset")]
        [Typing]
        public async Task Unset(string name)
        {
            var config = await Get(Context.Guild.Id);
            if (config.ContainsKey(name))
            {
                config.Remove(name);
            }
            await Set(Context.Guild.Id, config);
            await Context.ReactOk();
        }

        [Command("get")]
        public async Task Get()
        {
            var config = await Get(Context.Guild.Id);
            if (!config.HasValues)
            {
                await ReplyAsync("This server has no config set.");
                return;
            }

            var toReturn = string.Join("\n", config.Properties().Select(v =>
            {
                return $"{v.Name}: {v.Value}";
            }));
        }

        [Command("get")]
        public async Task Get(string name)
        {
            var conf = await Get(Context.Guild.Id);
            if (conf.ContainsKey(name))
            {
                await ReplyAsync($"{name}: `{conf[name]}`");
            }
            else
            {
                await ReplyAsync($"Nothing found in this server's config for `{name}`.");
            }
        }
    }
}

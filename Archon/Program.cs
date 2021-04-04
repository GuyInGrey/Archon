using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json.Linq;

using Templar;

namespace Archon
{
    public class Program
    {
        private static Bot Bot;

        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Current OS: {Environment.OSVersion}");

            var configPath = args.Length != 0 ? args[0] : @"config.json";
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Failed to find config file.\n{configPath}");
                Console.Read();
                return;
            }

            Log.OnLogPost += async (msg) => await PostLog(msg);

            var config = JObject.Parse(File.ReadAllText(configPath));
            var webhook = config["webhook"].Value<string>();
            var database = config["databaseConnection"].Value<string>();
            var token = config["token"].Value<string>();

            var getPrefix = new Func<SocketCommandContext, Task<string>>(async (context) =>
            {
                if (context.Guild is null) { return null; }
                var config = await ServerConfigLoader.Get(context.Guild.Id);
                if (config.ContainsKey("prefix"))
                {
                    return config["prefix"].ToString();
                }

                return null;
            });

            Console.WriteLine("Creating bot instance.");
            Bot = new Bot(webhook, database)
            {
                RunCommand = async (context) =>
                {
                    var prefix = await getPrefix(context);
                    if (prefix is null) { return true; }
                    return !await TagCmd.RunTag(context.Message, prefix);
                },
                GetPrefix = getPrefix,
            };

            Console.WriteLine("Starting bot!");
            await Bot.Start(token);
            Console.WriteLine("You should not see this text.");
        }

        private static async Task PostLog(Log msg)
        {
            PostToConsole(msg);

            try
            {
                IChannel channelToPost = null;
                if (msg.Server != 0)
                {
                    var conf = await ServerConfigLoader.Get(msg.Server);
                    if (conf.ContainsKey("botErrorLog") &&
                        ulong.TryParse(conf["botErrorLog"].ToString(), out var id))
                    {
                        channelToPost = Bot.Client.GetChannel(id);
                    }
                }
                else
                {
                    var guyingrey = Bot.Client.GetUser(126481324017057792);
                    if (guyingrey is null) { return; }
                    channelToPost = await guyingrey.GetOrCreateDMChannelAsync();
                }

                if (channelToPost is ITextChannel channel)
                {
                    var e = msg.ToEmbed(Bot.Client.CurrentUser).Build();
                    await channel.SendMessageAsync(embed: e);
                }
            } catch { }
        }

        private static void PostToConsole(Log log)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{log.Timestamp}] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(log.Title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(log.Content);
            log.Fields.ToList().ForEach(f =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(f.Key);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(f.Value);
            });

            if (log.Server != 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(log.Server);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(Bot.Client.GetGuild(log.Server) + " | " + log.Server);
            }

            Console.WriteLine();
        }
    }
}

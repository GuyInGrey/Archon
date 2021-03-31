using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using DiscordCommands = Discord.Commands.CommandService;

namespace Templar
{
    internal class CommandService
    {
        private static Bot _Bot;
        private static DiscordCommands _Commands;
        private static List<TypingState> _TypingStates;

        public CommandService(Bot bot)
        {
            Console.WriteLine("Starting Command Service..");
            _Bot = bot;
            _Commands = new DiscordCommands();
            _Commands.Log += async (a) => { bot.EventService.OnEvent("Log", a); };
            _Commands.CommandExecuted += async (a, b, c) => { bot.EventService.OnEvent("CommandExecuted", a, b, c); };
            _Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null).GetAwaiter().GetResult();
            Console.WriteLine("Done Starting Command Service");
        }

        [Event(Events.MessageReceived)]
        public static async Task BotMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg)) { return; }

            // Make sure it's prefixed (with ! or bot mention), and that caller isn't a bot
            var argPos = 0;
            var hasPrefix = msg.HasStringPrefix("!", ref argPos) || msg.HasMentionPrefix(_Bot.Client.CurrentUser, ref argPos);
            if (!(hasPrefix) || msg.Author.IsBot) { return; }

            var remainder = msg.Content.SplitAt(argPos).right;
            (var commandName, var suffix) = remainder.SplitAt(remainder.IndexOf(' '));

            var context = new SocketCommandContext(_Bot.Client, msg);
            var cmd = _Commands.Commands.FirstOrDefault(c => c.Name == commandName);
            if (!(cmd is null))
            {
                if (!(cmd.Attributes.FirstOrDefault(a => a is TypingAttribute) is null))
                {
                    _TypingStates.Add(new TypingState(msg.Id, msg.Channel.EnterTypingState()));
                }
            }

            if (_Bot.RunCommand is null || _Bot.RunCommand.Invoke(context))
            {
                await _Commands.ExecuteAsync(context, argPos, null);
            }
        }
    }

    public class TypingState
    {
        public ulong MessageID;
        public IDisposable TypingDisposable;

        public TypingState(ulong id, IDisposable dis)
        {
            MessageID = id;
            TypingDisposable = dis;
        }
    }
}

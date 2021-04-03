using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _Commands = new DiscordCommands(new CommandServiceConfig()
            {
                LogLevel = Discord.LogSeverity.Info,
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = false,
                SeparatorChar = ' ',
            });
            _Commands.Log += async (a) => { Console.WriteLine("BOB"); bot.EventService.OnEvent("Log", a); };
            _Commands.CommandExecuted += async (a, b, c) => { bot.EventService.OnEvent("CommandExecuted", a, b, c); };
            _Commands.AddModulesAsync(Assembly.GetEntryAssembly(), null).GetAwaiter().GetResult();
            Console.WriteLine("Done Starting Command Service");
        }

        [Event(Events.MessageReceived)]
        public static async Task BotMessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage msg) { return; }
            var context = new SocketCommandContext(_Bot.Client, msg);

            if (!(_Bot.RunCommand is null || await _Bot.RunCommand.Invoke(context)))
            {
                return;
            }

            string prefix = null;
            if (_Bot.GetPrefix is not null && !Debugger.IsAttached)
            {
                prefix = await _Bot.GetPrefix(context);
            }

            // Make sure it's prefixed (with ! or bot mention), and that caller isn't a bot
            var argPos = 0;
            var hasPrefix = (prefix != null && msg.HasStringPrefix(prefix, ref argPos)) || msg.HasMentionPrefix(_Bot.Client.CurrentUser, ref argPos);
            if (!(hasPrefix) || msg.Author.IsBot) { return; }

            var remainder = msg.Content.SplitAt(argPos).right;
            (var commandName, _) = remainder.SplitAt(remainder.IndexOf(' '));

            var cmd = _Commands.Commands.FirstOrDefault(c => c.Aliases.Any(a => a.StartsWith(commandName)));
            if (cmd is not null)
            {
                //if (cmd.Attributes.Any(a => a is TypingAttribute))
                //{
                //    _TypingStates.Add(new TypingState(msg.Id, msg.Channel.EnterTypingState()));
                //}
            }

            await _Commands.ExecuteAsync(context, argPos, null);
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

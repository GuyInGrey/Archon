using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Templar
{
    [Serializable]
    public class Log
    {
        public static Action<Log> OnLogPost;
        public static Bot Bot;

        public DateTime Timestamp = DateTime.Now;

        public string Title = "";
        public string Content = "";
        public Dictionary<string, string> Fields = new();
        public ulong Server = 0;

        public Color Color = Color.Green;

        public async Task Post()
        {
            OnLogPost?.Invoke(this);
            //PostToConsole();
            //var e = ToEmbed().Build();

            //// Get text channel
            //if (Client.GetChannel(ChannelToPost) is not SocketChannel channel) { return; }
            //if (channel is not ITextChannel ch) { return; }

            //await ch.SendMessageAsync(embed: e);
        }

        public Log WithField(string name, object content)
        {
            Fields.Add(name, content.ToString());
            return this;
        }

        public EmbedBuilder ToEmbed(IUser author)
        {
            var e = new EmbedBuilder()
                .WithTimestamp(new DateTimeOffset(Timestamp))
                .WithTitle(Title)
                .WithColor(Color);

            if (author is not null)
            {
                e.WithAuthor(author);
            }

            var con = Content.Trim();
            if (con.Length == 0) { }
            else if (con.Length <= 2040)
            {
                e.WithDescription("```\n" + con + "\n```");
            }
            else
            {
                var contentParts = con.SplitWithLength(1010);
                contentParts.ForEach(c => e.AddField("Content", $"```\n{c}\n```"));
            }

            var fields = Fields.Select(f => new EmbedFieldBuilder()
                .WithName(f.Key)
                .WithValue(f.Value)
                .WithIsInline(true));
            e.WithFields(fields);

            if (Server != 0)
            {
                e.AddField("Server", Bot.Client.GetGuild(Server).Name + " | " + Server);
            }

            if (Debugger.IsAttached)
            {
                e.WithFooter("Debug Instance");
            }

            return e;
        }

        public static Log FromException(Exception e)
        {
            var stackParts = (e.InnerException?.StackTrace ?? e.StackTrace).SplitWithLength(1000);
            if (e is CommandException ex)
            {
                var logEx = new Log()
                {
                    Server = ex.Context.Guild is null ? 0 : ex.Context.Guild.Id,
                    Title = ex.InnerException.GetType().Name,
                    Content = ex.InnerException.Message,
                    Color = Color.Orange,
                    Fields = new Dictionary<string, string>()
                    {
                        { "User", ex.Context.User.Username + "#" + ex.Context.User.Discriminator },
                        { "Location", ex.Context.Guild.Name + " > " + ex.Context.Channel.Name },
                        { "Command", ex.Context.Message.Content },
                    }
                };

                stackParts.ForEach(s => logEx.Fields.Add("Stack Trace", $"```\n{s}\n```"));
                return logEx;
            }

            var log = new Log()
            {
                Title = e.GetType().Name,
                Content = e.Message,
                Color = Color.Orange,
            };
            stackParts.ForEach(s => log.Fields.Add("Stack Trace", $"```\n{s}\n```"));
            return log;
        }

        public static Log InformationFromContext(ICommandContext c, string info)
        {
            return new Log()
            {
                Server = c.Guild is null ? 0 : c.Guild.Id,
                Title = "Info",
                Content = info,
                Color = Color.Blue,
                Fields = new Dictionary<string, string>()
                {
                    { "User", c.User.ToString() },
                    { "Location", c.Guild.Name + " > " + c.Channel.Name },
                    { "Command", c.Message.Content },
                },
            };
        }

        public static Log FromLogMessage(LogMessage m)
        {
            if (m.Exception is null)
            {
                return new Log()
                {
                    Title = m.Message,
                }.WithField("Severity", m.Severity);
            }

            if (m.Exception is GatewayReconnectException) { return null; }
            if (m.Exception.Message.Equals("WebSocket connection was closed")) { return null; }

            return FromException(m.Exception);
        }
    }
}

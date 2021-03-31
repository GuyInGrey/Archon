using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Templar
{
    public class Bot
    {
        internal DiscordSocketClient Client;
        internal EventService EventService;
        internal CommandService CommandService;

        public Func<SocketCommandContext, bool> RunCommand; 

        public Bot(string errorWebhook)
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LargeThreshold = 250,
                LogLevel = LogSeverity.Warning,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
                ExclusiveBulkDelete = true,
            });

            EventService = new EventService();
            CommandService = new CommandService(this);

            RegisterEvents();
        }

        // there HAS to be a better way to do this...
        internal void RegisterEvents()
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Client.ChannelCreated += async (a) => OnEvent("ChannelCreated", a);
            Client.ChannelDestroyed += async (a) => OnEvent("ChannelDestroyed", a);
            Client.ChannelUpdated += async (a, b) => OnEvent("ChannelUpdated", a, b);
            Client.Connected += async () => OnEvent("Connected");
            Client.CurrentUserUpdated += async (a, b) => OnEvent("CurrentUserUpdated", a, b);
            Client.Disconnected += async (a) => OnEvent("Disconnected", a);
            Client.GuildAvailable += async (a) => OnEvent("GuildAvailable", a);
            Client.GuildMembersDownloaded += async (a) => OnEvent("GuildMembersDownloaded", a);
            Client.GuildMemberUpdated += async (a, b) => OnEvent("GuildMemberUpdated", a, b);
            Client.GuildUnavailable += async (a) => OnEvent("GuildUnavailable", a);
            Client.GuildUpdated += async (a, b) => OnEvent("GuildUpdated", a, b);
            Client.InviteCreated += async (a) => OnEvent("InviteCreated", a);
            Client.InviteDeleted += async (a, b) => OnEvent("InviteDeleted", a, b);
            Client.JoinedGuild += async (a) => OnEvent("JoinedGuild", a);
            Client.LatencyUpdated += async (a, b) => OnEvent("LatencyUpdated", a, b);
            Client.LeftGuild += async (a) => OnEvent("LeftGuild", a);
            Client.Log += async (a) => OnEvent("Log", a);
            Client.LoggedIn += async () => OnEvent("LoggedIn");
            Client.LoggedOut += async () => OnEvent("LoggedOut");
            Client.MessageDeleted += async (a, b) => OnEvent("MessageDeleted", a, b);
            Client.MessageReceived += async (a) => OnEvent("MessageReceived", a);
            Client.MessagesBulkDeleted += async (a, b) => OnEvent("MessagesBulkDeleted", a, b);
            Client.MessageUpdated += async (a, b, c) => OnEvent("MessageUpdated", a, b, c);
            Client.ReactionAdded += async (a, b, c) => OnEvent("ReactionAdded", a, b, c);
            Client.ReactionRemoved += async (a, b, c) => OnEvent("ReactionRemoved", a, b, c);
            Client.ReactionsCleared += async (a, b) => OnEvent("ReactionsCleared", a, b);
            Client.ReactionsRemovedForEmote += async (a, b, c) => OnEvent("ReactionsRemovedForEmote", a, b, c);
            Client.Ready += async () => OnEvent("Ready");
            Client.RecipientAdded += async (a) => OnEvent("RecipientAdded", a);
            Client.RecipientRemoved += async (a) => OnEvent("RecipientRemoved", a);
            Client.RoleCreated += async (a) => OnEvent("RoleCreated", a);
            Client.RoleDeleted += async (a) => OnEvent("RoleDeleted", a);
            Client.RoleUpdated += async (a, b) => OnEvent("RoleUpdated", a, b);
            Client.UserBanned += async (a, b) => OnEvent("UserBanned", a, b);
            Client.UserIsTyping += async (a, b) => OnEvent("UserIsTyping", a, b);
            Client.UserJoined += async (a) => OnEvent("UserJoined", a);
            Client.UserLeft += async (a) => OnEvent("UserLeft", a);
            Client.UserUnbanned += async (a, b) => OnEvent("UserUnbanned", a, b);
            Client.UserUpdated += async (a, b) => OnEvent("UserUpdated", a, b);
            Client.UserVoiceStateUpdated += async (a, b, c) => OnEvent("UserVoiceStateUpdated", a, b, c);
            Client.VoiceServerUpdated += async (a) => OnEvent("VoiceServerUpdated", a);
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }

        internal void OnEvent(string name, params object[] args) => EventService.OnEvent(name, args);

        /// <summary>
        /// Infinitely blocking!
        /// </summary>
        /// <param name="token">The Discord bot token</param>
        public async Task Start(string token)
        {
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }
    }
}

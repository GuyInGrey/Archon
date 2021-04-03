using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using MySql.Data.MySqlClient;

namespace Templar
{
    public class Bot
    {
        public static Bot Instance;

        public DiscordSocketClient Client;
        internal EventService EventService;
        internal CommandService CommandService;
        internal static string DatabaseConnString;

        public Func<SocketCommandContext, Task<bool>> RunCommand;
        public Func<SocketCommandContext, Task<string>> GetPrefix;

        public bool FailedDatabaseCheck { get; }

        public Bot(string errorWebhook, string databaseConn)
        {
            Instance = this;
            Log.Bot = this;
            DatabaseConnString = databaseConn;

            Console.WriteLine("Testing database connection...");
            try
            {
                var conn = new MySqlConnection(DatabaseConnString);
                conn.Dispose();
            }
            catch (Exception e)
            {
                FailedDatabaseCheck = true;
                Console.WriteLine("Failed database connection test.");
                Log.FromException(e).Post().GetAwaiter().GetResult();
            }
            Console.WriteLine("Connection successful.");

            Console.WriteLine("Creating DiscordSocketClient...");
            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LargeThreshold = 250,
                LogLevel = LogSeverity.Info,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
                ExclusiveBulkDelete = true,
            });
            
            Console.WriteLine("Creating EventService...");
            EventService = new EventService();
            Console.WriteLine("Creating CommandService...");
            CommandService = new CommandService(this);

            Console.WriteLine("Registering Events...");
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
            Console.WriteLine("Done registering events.");
        }

        internal void OnEvent(string name, params object[] args) => EventService.OnEvent(name, args);

        /// <summary>
        /// Infinitely blocking!
        /// </summary>
        /// <param name="token">The Discord bot token</param>
        public async Task Start(string token)
        {
            if (FailedDatabaseCheck) { return; }

            Console.WriteLine("Logging in...");
            await Client.LoginAsync(TokenType.Bot, token);
            Console.WriteLine("Starting...");
            await Client.StartAsync();

            Console.WriteLine("Waiting Infitely...");
            await Task.Delay(-1);
            Console.WriteLine("You should not see this text.");
        }

        [Event(Events.Log)]
        public static async Task HandleLog(LogMessage msg)
        {
            await Log.FromLogMessage(msg).Post();
        }

        public static EmbedBuilder CreateEmbed() =>
            new EmbedBuilder()
            .WithCurrentTimestamp()
            .WithColor(Color.Blue)
            .WithAuthor(Bot.Instance.Client.CurrentUser);
    }
}

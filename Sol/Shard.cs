#region USING DIRECTIVES

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Sol.Common;
using Sol.Common.Converters;
using Sol.Database;
using Sol.Extensions;
using Sol.Modules.Administration.Services;
using Sol.Modules.Search.Services;

#endregion USING DIRECTIVES

namespace Sol
{
    public sealed class Shard
    {
        public static IReadOnlyList<(string Name, Command Command)> Commands;

        public static void UpdateCommandsList(CommandsNextExtension cnext)
        {
            Commands = cnext.GetAllRegisteredCommands()
                .Where(cmd => cmd.Parent is null)
                .SelectMany(cmd => cmd.Aliases.Select(alias => (alias, cmd)).Concat(new[] { (cmd.Name, cmd) }))
                .ToList()
                .AsReadOnly();
        }

        public int Id { get; }
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension CNext { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public VoiceNextExtension Voice { get; private set; }
        public DatabaseContextBuilder Database { get; private set; }
        public SharedData SharedData { get; private set; }

        public Shard(int sid, DatabaseContextBuilder dbb, SharedData shared)
        {
            this.Id = sid;
            this.Database = dbb;
            this.SharedData = shared;
        }

        public async Task StartAsync()
            => await this.Client.ConnectAsync();

        public async Task DisposeAsync()
        {
            await this.Client.DisconnectAsync();
            this.Client.Dispose();
        }

        public void Initialize(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
        {
            this.SetupClient(onGuildDownloadCompleted);
            this.SetupCommands();
            this.SetupInteractivity();
            this.SetupVoice();

            AsyncExecutionManager.RegisterEventListeners(this.Client, this);
        }

        private void SetupClient(AsyncEventHandler<GuildDownloadCompletedEventArgs> onGuildDownloadCompleted)
        {
            var cfg = new DiscordConfiguration()
            {
                Token = this.SharedData.Configuration.DiscordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = this.SharedData.Configuration.LogLevel,
                UseInternalLogHandler = false,
                MessageCacheSize = 5000,
                ShardId = this.Id,
                ShardCount = this.SharedData.Configuration.ShardCount
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.OSVersion.Version <= new Version(6, 1, 7601, 65536))
                cfg.WebSocketClientFactory = WebSocket4NetCoreClient.CreateNew;

            this.Client = new DiscordClient(cfg);

            this.Client.DebugLogger.LogMessageReceived += (s, e) => {
                this.SharedData.LogProvider.Log(this.Id, e);
            };
            this.Client.Ready += e => {
                this.SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Ready!", this.Id);
                return Task.CompletedTask;
            };
            this.Client.GuildDownloadCompleted += onGuildDownloadCompleted;
        }

        private void SetupCommands()
        {
            this.CNext = this.Client.UseCommandsNext(new CommandsNextConfiguration
            {
                EnableDms = false,
                CaseSensitive = false,
                EnableMentionPrefix = true,
                PrefixResolver = this.PrefixResolverAsync,
                Services = new ServiceCollection()
                    .AddSingleton(this)
                    .AddSingleton(this.SharedData)
                    .AddSingleton(this.Database)
                    .AddSingleton(new AntifloodService(this))
                    .AddSingleton(new AntiInstantLeaveService(this))
                    .AddSingleton(new AntispamService(this))
                    .AddSingleton(new GiphyService(this.SharedData.Configuration.GiphyKey))
                    .AddSingleton(new GoodreadsService(this.SharedData.Configuration.GoodreadsKey))
                    .AddSingleton(new ImgurService(this.SharedData.Configuration.ImgurKey))
                    .AddSingleton(new LinkfilterService(this))
                    .AddSingleton(new OMDbService(this.SharedData.Configuration.OMDbKey))
                    .AddSingleton(new RatelimitService(this))
                    .AddSingleton(new SteamService(this.SharedData.Configuration.SteamKey))
                    .AddSingleton(new WeatherService(this.SharedData.Configuration.WeatherKey))
                    .AddSingleton(new YtService(this.SharedData.Configuration.YouTubeKey))
                    .BuildServiceProvider()
            });

            this.CNext.SetHelpFormatter<CustomHelpFormatter>();

            this.CNext.RegisterCommands(Assembly.GetExecutingAssembly());

            this.CNext.RegisterConverter(new CustomActivityTypeConverter());
            this.CNext.RegisterConverter(new CustomBoolConverter());
            this.CNext.RegisterConverter(new CustomTimeWindowConverter());
            this.CNext.RegisterConverter(new CustomIPAddressConverter());
            this.CNext.RegisterConverter(new CustomIPFormatConverter());
            this.CNext.RegisterConverter(new CustomPunishmentActionTypeConverter());

            UpdateCommandsList(this.CNext);
        }

        private void SetupInteractivity()
        {
            var cfg = new InteractivityConfiguration {
                PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            };

            this.Client.UseInteractivity(cfg);
        }

        private void SetupVoice()
        {
            this.Voice = this.Client.UseVoiceNext();
        }

        private Task<int> PrefixResolverAsync(DiscordMessage m)
        {
            string p = this.SharedData.GetGuildPrefix(m.Channel.Guild.Id) ?? this.SharedData.Configuration.DefaultPrefix;
            return Task.FromResult(m.GetStringPrefixLength(p));
        }
    }
}

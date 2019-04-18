#region USING DIRECTIVES

using DSharpPlus;
using DSharpPlus.Entities;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Sol.Common.Collections;
using Sol.Modules.Administration.Common;
using Sol.Modules.Reactions.Common;

#endregion USING DIRECTIVES

namespace Sol.Common
{
    public sealed class SharedData : IDisposable
    {
        public AsyncExecutor AsyncExecutor { get; }
        public ConcurrentHashSet<ulong> BlockedChannels { get; internal set; }
        public ConcurrentHashSet<ulong> BlockedUsers { get; internal set; }
        public BotConfig Configuration { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>> EmojiReactions { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>> Filters { get; internal set; }
        public ConcurrentDictionary<ulong, CachedGuildConfig> GuildConfigurations { get; internal set; }
        public Logger LogProvider { get; internal set; }
        public bool ListeningStatus { get; internal set; }
        public CancellationTokenSource MainLoopCts { get; internal set; }
        public ConcurrentDictionary<ulong, int> Messages { get; internal set; }
        public bool StatusRotationEnabled { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>> RemindExecuters { get; internal set; }
        public ConcurrentDictionary<int, SavedTaskExecutor> TaskExecuters { get; internal set; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>> TextReactions { get; internal set; }
        public UptimeInformation UptimeInformation { get; internal set; }

        private ConcurrentDictionary<ulong, ChannelEvent> ChannelEvents { get; }
        private ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> PendingResponses { get; }

        public SharedData()
        {
            this.AsyncExecutor = new AsyncExecutor();
            this.BlockedChannels = new ConcurrentHashSet<ulong>();
            this.BlockedUsers = new ConcurrentHashSet<ulong>();
            this.ChannelEvents = new ConcurrentDictionary<ulong, ChannelEvent>();
            this.EmojiReactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>();
            this.Filters = new ConcurrentDictionary<ulong, ConcurrentHashSet<Filter>>();
            this.GuildConfigurations = new ConcurrentDictionary<ulong, CachedGuildConfig>();
            this.ListeningStatus = true;
            this.MainLoopCts = new CancellationTokenSource();
            this.PendingResponses = new ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>>();
            this.RemindExecuters = new ConcurrentDictionary<ulong, ConcurrentDictionary<int, SavedTaskExecutor>>();
            this.StatusRotationEnabled = true;
            this.TaskExecuters = new ConcurrentDictionary<int, SavedTaskExecutor>();
            this.TextReactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>();
            this.Configuration = BotConfig.Default;
            this.MainLoopCts = new CancellationTokenSource();
            this.Messages = new ConcurrentDictionary<ulong, int>();
        }

        public void Dispose()
        {
            this.MainLoopCts.Dispose();
        }

        #region GUILD_DATA_HELPERS
        public CachedGuildConfig GetGuildConfig(ulong gid)
            => this.GuildConfigurations.GetOrAdd(gid, CachedGuildConfig.Default);

        public string GetGuildPrefix(ulong gid)
        {
            if (this.GuildConfigurations.TryGetValue(gid, out CachedGuildConfig gcfg) && !string.IsNullOrWhiteSpace(gcfg.Prefix))
                return this.GuildConfigurations[gid].Prefix;
            else
                return this.Configuration.DefaultPrefix;
        }

        public DiscordChannel GetLogChannelForGuild(DiscordClient client, DiscordGuild guild)
        {
            CachedGuildConfig gcfg = this.GetGuildConfig(guild.Id);
            return gcfg.LoggingEnabled ? guild.GetChannel(gcfg.LogChannelId) : null;
        }

        public bool GuildHasTextReaction(ulong gid, string trigger)
            => this.TextReactions.TryGetValue(gid, out var trs) && (trs?.Any(tr => tr.ContainsTriggerPattern(trigger)) ?? false);

        public bool MessageContainsFilter(ulong gid, string message)
        {
            if (!this.Filters.TryGetValue(gid, out var filters) || filters is null)
                return false;

            message = message.ToLowerInvariant();
            return filters.Any(f => f.Trigger.IsMatch(message));
        }

        public void UpdateGuildConfig(ulong gid, Func<CachedGuildConfig, CachedGuildConfig> modifier)
            => this.GuildConfigurations[gid] = modifier(this.GuildConfigurations[gid]);
        #endregion

    }
}

﻿#region USING DIRECTIVES

using DSharpPlus.Entities;

using System;
using System.Linq;
using System.Threading.Tasks;

using Sol.Database;
using Sol.Database.Models;

#endregion USING DIRECTIVES

namespace Sol.Extensions
{
    internal static class DiscordGuildExtensions
    {
        public static async Task<DiscordAuditLogEntry> GetLatestAuditLogEntryAsync(this DiscordGuild guild, AuditLogActionType type)
        {
            try
            {
                var entry = (await guild.GetAuditLogsAsync(1, action_type: type))
                    ?.FirstOrDefault();
                if (entry is null || DateTime.UtcNow - entry.CreationTimestamp.ToUniversalTime() > TimeSpan.FromSeconds(5))
                    return null;
                return entry;
            }
            catch
            {
            }
            return null;
        }

        public static DatabaseGuildConfig GetGuildSettings(this DiscordGuild guild, DatabaseContextBuilder dbb)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = dbb.CreateContext())
                gcfg = guild.GetGuildConfig(db);
            return gcfg;
        }

        public static DatabaseGuildConfig GetGuildConfig(this DiscordGuild guild, DatabaseContext db)
            => db.GuildConfig.SingleOrDefault(cfg => cfg.GuildId == guild.Id);
    }
}
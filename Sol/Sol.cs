#region USING DIRECTIVES

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Sol.Common;
using Sol.Database;
using Sol.Database.Models;
using Sol.Modules.Search.Services;
using Sol.Extensions;

using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

#endregion USING DIRECTIVES

namespace Sol
{
    internal static class Sol
    {
        private static BotConfig Configuration { get; set; }
        private static SharedData SharedData { get; set; }
        private static DiscordClient Client { get; set; }
        private static List<Shard> Shards { get; set; }
        private static DatabaseContextBuilder GlobalDatabaseContextBuilder { get; set; }

        public static string ApplicationName { get; } = "Solstice";
        public static string ApplicationVersion { get; } = "v1.0.0-snapshot";
        public static ushort ApplicationRevision { get; } = 1;
        public static string ConfigPath { get; } = "Resources/config.json";
        public static IReadOnlyList<Shard> ActiveShards => Shards.AsReadOnly();

        #region TIMERS
        private static Timer BotStatusUpdateTimer { get; set; }
        private static Timer DatabaseSyncTimer { get; set; }
        private static Timer FeedCheckTimer { get; set; }
        private static Timer MiscActionsTimer { get; set; }
        #endregion

        internal static async Task Main()
        {
            try
            {
                PrintBuildInformation();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                await LoadBotConfigAsync();
                await LoadShards();

                try
                {
                    await Task.Delay(Timeout.Infinite, SharedData.MainLoopCts.Token);
                } catch (TaskCanceledException)
                {
                    Console.WriteLine("\rShutdown signal received!          ");
                }

                await DisposeAsync();
            } catch (Exception e)
            {
                Console.WriteLine($"{e.GetType()} :{e.Message}");
                if (!(e.InnerException is null))
                {
                    Console.WriteLine($"{e.InnerException.GetType()} :\n\n                     ");
                    Console.WriteLine($"{e.InnerException.Source} :\n{e.InnerException.Message}");
                }
            }

            Console.ReadKey();
            Console.WriteLine("Shutting down...             ");
        }

        private static void PrintBuildInformation()
        {
            var a = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(a.Location);

            Console.WriteLine($"{ ApplicationName } { ApplicationVersion } [Revision: { ApplicationRevision }] ({ fvi })");
            Console.WriteLine();
        }

        private static async Task LoadBotConfigAsync()
        {
            Console.WriteLine("Loading bot configuration...             ");

            string json = "{}";
            var utf8 = new UTF8Encoding(false);
            var fi = new FileInfo("Resources/config.json");
            if (!fi.Exists)
            {
                Console.WriteLine("[ERROR] Failed loading configuration!            ");
                Console.WriteLine("[INFO] One will be created at:                   ");
                Console.WriteLine($"[INFO] {ConfigPath}                             ");
                Console.WriteLine("[INFO] Fill in with the appropriate values!      ");

                json = JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented);
                using (FileStream fs = fi.Create())
                {
                    using (var sw = new StreamWriter(fs, utf8))
                    {
                        await sw.WriteAsync(json);
                        await sw.FlushAsync();
                    }
                }

                throw new IOException("Configuration file not found!");
            }

            using (FileStream fs = fi.OpenRead())
            {
                using (var sr = new StreamReader(fs, utf8))
                {
                    json = await sr.ReadToEndAsync();
                }
            }

            Configuration = JsonConvert.DeserializeObject<BotConfig>(json);
        }

        private static AsyncEventHandler<GuildDownloadCompletedEventArgs> OnGuildDownloadCompleted;

        private static Task LoadShards()
        {
            Shards = new List<Shard>();
            for(int i = 0; i < Configuration.ShardCount; i++)
            {
                var shard = new Shard(i, GlobalDatabaseContextBuilder, SharedData);
                shard.Initialize(async e => await RegisterPeriodicTasksAsync());
                Shards.Add(shard);
            }

            Console.WriteLine("\r[5/5] Booting the shards...                   ");
            Console.WriteLine();

            return Task.WhenAll(Shards.Select(s => s.StartAsync()));
        }

        private static async Task RegisterPeriodicTasksAsync()
        {
            BotStatusUpdateTimer = new Timer(BotActivityCallback, Shards[0].Client, TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(10));
            DatabaseSyncTimer = new Timer(DatabaseSyncCallback, Shards[0].Client, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(Configuration.DatabaseSyncInterval));
            FeedCheckTimer = new Timer(FeedCheckCallback, Shards[0].Client, TimeSpan.FromSeconds(Configuration.FeedCheckStartDelay), TimeSpan.FromSeconds(Configuration.FeedCheckInterval));
            MiscActionsTimer = new Timer(MiscellaneousActionsCallback, Shards[0].Client, TimeSpan.FromSeconds(5), TimeSpan.FromHours(12));

            using (DatabaseContext db = GlobalDatabaseContextBuilder.CreateContext())
            {
                await RegisterSavedTasksAsync(db.SavedTasks.ToDictionary<DatabaseSavedTask, int, SavedTaskInfo>(
                    t => t.Id,
                    t => {
                        switch (t.Type)
                        {
                            case SavedTaskType.Unban:
                                return new UnbanTaskInfo(t.GuildId, t.UserId, t.ExecutionTime);
                            case SavedTaskType.Unmute:
                                return new UnmuteTaskInfo(t.GuildId, t.UserId, t.RoleId, t.ExecutionTime);
                            default:
                                return null;
                        }
                    })
                );
                await RegisterRemindersAsync(db.Reminders.ToDictionary(
                    t => t.Id,
                    t => new SendMessageTaskInfo(t.ChannelId, t.UserId, t.Message, t.ExecutionTime, t.IsRepeating, t.RepeatInterval)
                ));
            }


            async Task RegisterSavedTasksAsync(IReadOnlyDictionary<int, SavedTaskInfo> tasks)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SavedTaskInfo task) in tasks)
                {
                    if (await RegisterTaskAsync(tid, task))
                        scheduled++;
                    else
                        missed++;
                }
                SharedData.LogProvider.ElevatedLog(LogLevel.Info, $"Saved tasks: {scheduled} scheduled; {missed} missed.");
            }

            async Task RegisterRemindersAsync(IReadOnlyDictionary<int, SendMessageTaskInfo> reminders)
            {
                int scheduled = 0, missed = 0;
                foreach ((int tid, SendMessageTaskInfo task) in reminders)
                {
                    if (await RegisterTaskAsync(tid, task))
                        scheduled++;
                    else
                        missed++;
                }
                SharedData.LogProvider.ElevatedLog(LogLevel.Info, $"Reminders: {scheduled} scheduled; {missed} missed.");
            }

            async Task<bool> RegisterTaskAsync(int id, SavedTaskInfo tinfo)
            {
                var texec = new SavedTaskExecutor(id, Shards[0].Client, tinfo, SharedData, GlobalDatabaseContextBuilder);
                if (texec.TaskInfo.IsExecutionTimeReached)
                {
                    await texec.HandleMissedExecutionAsync();
                    return false;
                }
                else
                {
                    texec.Schedule();
                    return true;
                }
            }
        }

        private static async Task DisposeAsync()
        {
            SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Cleaning up...");

            BotStatusUpdateTimer.Dispose();
            DatabaseSyncTimer.Dispose();
            FeedCheckTimer.Dispose();
            MiscActionsTimer.Dispose();

            foreach (Shard shard in Shards)
                await shard.DisposeAsync();
            SharedData.Dispose();

            SharedData.LogProvider.ElevatedLog(LogLevel.Info, "Cleanup complete! Powering off...");
        }

        #region PERIODIC_CALLBACKS
        private static void BotActivityCallback(object _)
        {
            if (!SharedData.StatusRotationEnabled)
                return;

            var client = _ as DiscordClient;

            try
            {
                DatabaseBotStatus status;
                using (DatabaseContext db = GlobalDatabaseContextBuilder.CreateContext())
                    status = db.BotStatuses.Shuffle().FirstOrDefault();

                var activity = new DiscordActivity(status?.Status ?? "@TheGodfather help", status?.Activity ?? ActivityType.Playing);

                SharedData.AsyncExecutor.Execute(client.UpdateStatusAsync(activity));
            }
            catch (Exception e)
            {
                SharedData.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void DatabaseSyncCallback(object _)
        {
            try
            {
                using (DatabaseContext db = GlobalDatabaseContextBuilder.CreateContext())
                {
                    foreach ((ulong uid, int count) in SharedData.Messages)
                    {
                        DatabaseMessageCount msgcount = db.MessageCount.Find((long)uid);
                        if (msgcount is null)
                        {
                            db.MessageCount.Add(new DatabaseMessageCount()
                            {
                                MessageCount = count,
                                UserId = uid
                            });
                        }
                        else
                        {
                            if (count != msgcount.MessageCount)
                            {
                                msgcount.MessageCount = count;
                                db.MessageCount.Update(msgcount);
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                SharedData.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void FeedCheckCallback(object _)
        {
            var client = _ as DiscordClient;

            try
            {
                SharedData.AsyncExecutor.Execute(RssService.CheckFeedsForChangesAsync(client, GlobalDatabaseContextBuilder));
            }
            catch (Exception e)
            {
                SharedData.LogProvider.Log(LogLevel.Error, e);
            }
        }

        private static void MiscellaneousActionsCallback(object _)
        {
            var client = _ as DiscordClient;

            try
            {
                List<DatabaseBirthday> todayBirthdays;
                using (DatabaseContext db = GlobalDatabaseContextBuilder.CreateContext())
                {
                    todayBirthdays = db.Birthdays
                        .Where(b => b.Date.Month == DateTime.Now.Month && b.Date.Day == DateTime.Now.Day && b.LastUpdateYear < DateTime.Now.Year)
                        .ToList();
                }
                foreach (DatabaseBirthday birthday in todayBirthdays)
                {
                    DiscordChannel channel = SharedData.AsyncExecutor.Execute(client.GetChannelAsync(birthday.ChannelId));
                    DiscordUser user = SharedData.AsyncExecutor.Execute(client.GetUserAsync(birthday.UserId));
                    SharedData.AsyncExecutor.Execute(channel.SendMessageAsync(user.Mention, embed: new DiscordEmbedBuilder()
                    {
                        Description = $"{StaticDiscordEmoji.Tada} Happy birthday, {user.Mention}! {StaticDiscordEmoji.Cake}",
                        Color = DiscordColor.Aquamarine
                    }));

                    using (DatabaseContext db = GlobalDatabaseContextBuilder.CreateContext())
                    {
                        birthday.LastUpdateYear = DateTime.Now.Year;
                        db.Birthdays.Update(birthday);
                        db.SaveChanges();
                    }
                }

                using (DatabaseContext db = GlobalDatabaseContextBuilder.CreateContext())
                {
                    db.Database.ExecuteSqlCommand("UPDATE gf.bank_accounts SET balance = GREATEST(CEILING(1.0015 * balance), 10);");
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                SharedData.LogProvider.Log(LogLevel.Error, e);
            }
        }
        #endregion
    }
}

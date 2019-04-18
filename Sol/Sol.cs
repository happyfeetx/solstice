#region USING DIRECTIVES

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using Sol.Common;

using DSharpPlus;
using DSharpPlus.Entities;

#endregion USING DIRECTIVES

namespace Sol
{
    internal static class Sol
    {
        public static BotConfig Configuration { get; internal set; }
        public static SharedData SharedData { get; internal set; }
        public static DiscordClient Client { get; internal set; }
        public static Shard Shard { get; internal set; }

        public static string ApplicationName { get; } = "Solstice";
        public static string ApplicationVersion { get; } = "v1.0.0-snapshot";
        public static ushort ApplicationRevision { get; } = 1;
        public static string ConfigPath { get; } = "Resources/config.json";

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

        private static async Task LoadShards()
        {

        }

        private static async Task DisposeAsync()
        {
            SharedData.Dispose();

            await Shard.DisposeAsync();
        }
    }
}

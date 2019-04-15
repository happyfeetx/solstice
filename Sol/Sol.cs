#region USING DIRECTIVES

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Sol.Common;

#endregion USING DIRECTIVES

namespace Sol
{
    internal static class Sol
    {
        public static BotConfig Configuration { get; internal set; }
        public static SharedData SharedData { get; internal set; }

        public static string ApplicationName { get; } = "Solstice";
        public static string ApplicationVersion { get; } = "v1.0.0-snapshot";
        public static short ApplicationRevision { get; } = 1;
        public static string ConfigPath { get; } = "Resources/config.json";

        internal static async Task Main()
        {
            try
            {
                PrintBuildInformation();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                await LoadBotConfigAsync();

            } catch (Exception e)
            {

            }
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
                Console.WriteLine("[ERROR] One will be created at:                  ");
                Console.WriteLine("[ERROR] " + ConfigPath);
                Console.WriteLine("[ERROR] Fill in with the appropriate values!     ");

                json = JsonConvert.SerializeObject(BotConfig.Default, Formatting.Indented);
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
    }
}

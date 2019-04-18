#region USING DIRECTIVES

using DSharpPlus;
using Newtonsoft.Json;

#endregion USING DIRECTIVES

namespace Sol.Common
{
    public sealed class BotConfig
    {
        [JsonProperty("discord-token")]
        public string DiscordToken { get; private set; }

        [JsonProperty("default-prefix")]
        public string DefaultPrefix { get; private set; }

        [JsonProperty("steam-token")]
        public string SteamToken { get; private set; }

        [JsonProperty("youtube-token")]
        public string YoutubeToken { get; private set; }

        [JsonProperty("riot-token")]
        public string RiotToken { get; private set; }

        [JsonProperty("bungie-token")]
        public string BungieToken { get; private set; }

        [JsonProperty("database-provider")]
        public DatabaseProvider Provider { get; private set; }

        [JsonProperty("log-level")]
        public LogLevel LogLevel { get; private set; }

        [JsonProperty("log-to-file")]
        public bool LogToFile { get; private set; }

        [JsonProperty("log-path")]
        public string LogPath { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonProperty("hostname")]
        public string Hostname { get; private set; }

        [JsonProperty("db-name")]
        public string DatabaseName { get; private set; }

        [JsonProperty("port")]
        public ushort Port { get; private set; }

        [JsonIgnore]
        public static BotConfig Default => new BotConfig()
        {
            DiscordToken = "<Discord API Token>",
            SteamToken = "<Steam API Token>",
            BungieToken = "<Bungie API Token>",
            RiotToken = "<Riot Gaames API Token>",
            YoutubeToken = "<YouTube API Token>",
            LogLevel = LogLevel.Info,
            LogToFile = false,
            LogPath = "Resources/log.txt"
        };

        [JsonIgnore]
        public static BotConfig Database => new BotConfig()
        {
            Hostname = "localhost",
            Port = 5432,
            DatabaseName = "SolsticeDB",
            Username = "Database",
            Password = ""
        };

    }

    public enum DatabaseProvider
    {
        SQLite = 0,
        PostgreSQL = 1,
        SQLServer = 2,
        Cosmos = 3,
        InMemory = 4
    }
}

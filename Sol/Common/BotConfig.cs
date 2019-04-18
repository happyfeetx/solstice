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

        [JsonProperty("shard-count")]
        public int ShardCount { get; private set; }

        [JsonProperty("steam-key")]
        public string SteamKey { get; private set; }
        
        [JsonProperty("imgur-key")]
        public string ImgurKey { get; private set; }

        [JsonProperty("omdb-key")]
        public string OMDbKey { get; private set; }

        [JsonProperty("youtube-key")]
        public string YouTubeKey { get; private set; }

        [JsonProperty("weather-key")]
        public string WeatherKey { get; private set; }

        [JsonProperty("giphy-key")]
        public string GiphyKey { get; private set; }


        [JsonIgnore]
        public static BotConfig Default => new BotConfig()
        {
            DiscordToken = "<Discord API Token>",
            SteamKey = "<Steam API Token>",
            BungieToken = "<Bungie API Token>",
            RiotToken = "<Riot Gaames API Token>",
            YouTubeKey = "<YouTube API Token>",
            GiphyKey = "<Giphy API Token>",
            ImgurKey = "<Imgur API Token>",
            WeatherKey = "<OpenWeatherMaps API Key>",
            LogLevel = LogLevel.Info,
            LogToFile = false,
            LogPath = "Resources/log.txt",
            ShardCount = 1
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

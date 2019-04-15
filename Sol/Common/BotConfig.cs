#region USING DIRECTIVES

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

        [JsonIgnore]
        public static BotConfig Default => new BotConfig()
        {
            DiscordToken = "<Discord API Token>",
            SteamToken = "<Steam API Token>",
            BungieToken = "<Bungie API Token>",
            RiotToken = "<Riot Gaames API Token>",
            YoutubeToken = "<YouTube API Token>"
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

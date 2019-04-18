﻿#region USING_DIRECTIVES
using Sol.Modules.Administration.Common;
#endregion

namespace Sol.Common
{
    public sealed class CachedGuildConfig
    {
        public string Currency { get; set; }
        public string Prefix { get; set; }
        public ulong LogChannelId { get; set; }
        public bool SuggestionsEnabled { get; set; }
        public bool ReactionResponse { get; set; }

        public LinkfilterSettings LinkfilterSettings { get; set; }
        public AntispamSettings AntispamSettings { get; set; }
        public RatelimitSettings RatelimitSettings { get; set; }

        public bool LoggingEnabled => this.LogChannelId != default;


        public static CachedGuildConfig Default => new CachedGuildConfig
        {
            AntispamSettings = new AntispamSettings(),
            Currency = null,
            LinkfilterSettings = new LinkfilterSettings(),
            LogChannelId = default,
            Prefix = null,
            RatelimitSettings = new RatelimitSettings(),
            ReactionResponse = false,
            SuggestionsEnabled = false
        };
    }
}

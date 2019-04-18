﻿#region USING_DIRECTIVES

using Newtonsoft.Json;

#endregion USING_DIRECTIVES

namespace Sol.Modules.Search.Common
{
    public class XkcdComic
    {
        [JsonProperty("num")]
        public int Id { get; set; }

        [JsonProperty("img")]
        public string ImageUrl { get; set; }

        [JsonProperty("month")]
        public string Month { get; set; }

        [JsonProperty("safe_title")]
        public string Title { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }
    }
}
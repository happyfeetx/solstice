﻿#region USING_DIRECTIVES

using Newtonsoft.Json;

#endregion USING_DIRECTIVES

namespace Sol.Modules.Search.Common
{
    public class IpInfo
    {
        [JsonIgnore]
        public bool Success => this.Status == "success";

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("query")]
        public string Ip { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("country")]
        public string CountryName { get; set; }

        [JsonProperty("regionCode")]
        public string RegionCode { get; set; }

        [JsonProperty("region")]
        public string RegionName { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string ZipCode { get; set; }

        [JsonProperty("lat")]
        public float Latitude { get; set; }

        [JsonProperty("lon")]
        public float Longitude { get; set; }

        [JsonProperty("isp")]
        public string Isp { get; set; }

        [JsonProperty("as")]
        public string As { get; set; }

        [JsonProperty("org")]
        public string Organization { get; set; }
    }
}
﻿#region USING_DIRECTIVES

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Sol.Services;

#endregion USING_DIRECTIVES

namespace Sol.Modules.Search.Services
{
    public class JokesService : KioskHttpService
    {
        private static readonly string _url = "https://icanhazdadjoke.com";

        public override bool IsDisabled()
            => false;

        public static Task<string> GetRandomJokeAsync()
            => ReadResponseAsync(_url);

        public static async Task<string> GetRandomYoMommaJokeAsync()
        {
            string data = await _http.GetStringAsync("https://api.yomomma.info/").ConfigureAwait(false);
            return JObject.Parse(data)["joke"].ToString();
        }

        public static async Task<IReadOnlyList<string>> SearchForJokesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query missing!", nameof(query));

            string res = await ReadResponseAsync($"{_url}/search?term={WebUtility.UrlEncode(query)}").ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(res))
                return null;
            return res.Split('\n').ToList().AsReadOnly();
        }

        private static async Task<string> ReadResponseAsync(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/plain";

            string data = null;
            using (var response = await request.GetResponseAsync().ConfigureAwait(false))
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                data = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            return data;
        }
    }
}
﻿#region USING_DIRECTIVES

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Sol.Modules.Search.Common;
using Sol.Services;

#endregion USING_DIRECTIVES

namespace Sol.Modules.Search.Services
{
    public class QuoteService : KioskHttpService
    {
        private static readonly Regex _tagMatcher = new Regex("<.*?>", RegexOptions.Compiled);
        private static readonly string _url = "https://quotes.rest/qod.json";

        public override bool IsDisabled()
            => false;

        public static async Task<Quote> GetQuoteOfTheDayAsync(string category = null)
        {
            string response = null;
            if (string.IsNullOrWhiteSpace(category))
                response = await _http.GetStringAsync(_url).ConfigureAwait(false);
            else
                response = await _http.GetStringAsync($"{_url}?category={WebUtility.UrlEncode(category)}").ConfigureAwait(false);

            var data = JsonConvert.DeserializeObject<QuoteApiResponse>(response);
            return data?.Contents?.Quotes?.FirstOrDefault();
        }

        public static async Task<string> GetRandomQuoteAsync()
        {
            string response = await _http.GetStringAsync("http://quotesondesign.com/wp-json/posts?filter[orderby]=rand&filter[posts_per_page]=1").ConfigureAwait(false);
            string data = JArray.Parse(response).First["content"].ToString();
            data = _tagMatcher.Replace(data, string.Empty);
            return WebUtility.HtmlDecode(data).Trim();
        }
    }
}
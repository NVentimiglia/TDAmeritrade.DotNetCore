using System;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace TDAmeritrade
{
    /// <summary>
    /// Request real-time and delayed top level quote data
    /// https://developer.tdameritrade.com/quotes/apis
    /// </summary>
    public class TDQuoteClient
    {
        TDAuthClient _auth;

        public TDQuoteClient(TDAuthClient auth)
        {
            _auth = auth;
        }

        public Task<EquityQuote> GetQuote_Equity(string symbol) => GetQuote<EquityQuote>(symbol);
        public Task<ETFQuote> GetQuote_ETF(string symbol) => GetQuote<ETFQuote>(symbol);
        public Task<IndexQuote> GetQuote_Index(string symbol) => GetQuote<IndexQuote>(symbol);
        public Task<FutureQuote> GetQuote_Future(string symbol) => GetQuote<FutureQuote>(symbol);
        public Task<FutureOptionsQuote> GetQuote_FutureOption(string symbol) => GetQuote<FutureOptionsQuote>(symbol);
        public Task<OptionQuote> GetQuote_Option(string symbol) => GetQuote<OptionQuote>(symbol);
        public Task<ForexQuote> GetQuote_Forex(string symbol) => GetQuote<ForexQuote>(symbol);

        /// <summary>
        /// Get quote for a symbol
        /// https://developer.tdameritrade.com/quotes/apis/get/marketdata/%7Bsymbol%7D/quotes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<T> GetQuote<T>(string symbol) where T : QuoteBase
        {
            var json = await GetQuoteJson(symbol);
            if (!string.IsNullOrEmpty(json))
            {
                var doc = JsonDocument.Parse(json);
                var betterSymbol = symbol.StartsWith("/") ? symbol.Substring(1) : symbol;
                var inner = doc.RootElement.GetProperty(betterSymbol).GetRawText();
                return JsonSerializer.Deserialize<T>(inner);
            }
            return null;
        }

        /// <summary>
        /// Get quote for a symbol
        /// https://developer.tdameritrade.com/quotes/apis/get/marketdata/%7Bsymbol%7D/quotes
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<string> GetQuoteJson(string symbol)
        {
            if (!_auth.HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            var key = HttpUtility.UrlEncode(_auth.Result.consumer_key);

            string path = _auth.IsSignedIn
                ? $"https://api.tdameritrade.com/v1/marketdata/{symbol}/quotes"
                : $"https://api.tdameritrade.com/v1/marketdata/{symbol}/quotes?apikey={key}";

            using (var client = new HttpClient())
            {
                if (_auth.IsSignedIn)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Result.access_token);
                }
                var res = await client.GetAsync(path);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return await res.Content.ReadAsStringAsync();
                    default:
                        Console.WriteLine("Error: " + res.ReasonPhrase);
                        return null;
                }
            }
        }
    }
}

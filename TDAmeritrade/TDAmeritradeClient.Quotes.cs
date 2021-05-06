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
    public partial class TDAmeritradeClient
    {
        /// <summary>
        /// Helper
        /// </summary>
        public static bool IsFutureSymbol(string symbol)
        {
            return symbol.StartsWith("/");
        }

        /// <summary>
        /// Helper
        /// </summary>
        public static bool IsIndexSymbol(string symbol)
        {
            return symbol.StartsWith("$");
        }

        public Task<TDEquityQuote> GetQuote_Equity(string symbol) => GetQuote<TDEquityQuote>(symbol);
        public Task<TDIndexQuote> GetQuote_Index(string symbol) => GetQuote<TDIndexQuote>(symbol);
        public Task<FutureQuote> GetQuote_Future(string symbol) => GetQuote<FutureQuote>(symbol);
        public Task<FutureOptionsQuote> GetQuote_FutureOption(string symbol) => GetQuote<FutureOptionsQuote>(symbol);
        public Task<TDOptionQuote> GetQuote_Option(string symbol) => GetQuote<TDOptionQuote>(symbol);
        public Task<TDForexQuote> GetQuote_Forex(string symbol) => GetQuote<TDForexQuote>(symbol);

        /// <summary>
        /// Get quote for a symbol
        /// https://developer.tdameritrade.com/quotes/apis/get/marketdata/%7Bsymbol%7D/quotes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async Task<T> GetQuote<T>(string symbol) where T : TDQuoteBase
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
            if (!HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            var key = HttpUtility.UrlEncode(AuthResult.consumer_key);

            string path = IsSignedIn
                ? $"https://api.tdameritrade.com/v1/marketdata/{symbol}/quotes"
                : $"https://api.tdameritrade.com/v1/marketdata/{symbol}/quotes?apikey={key}";

            using (var client = new HttpClient())
            {
                if (IsSignedIn)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthResult.access_token);
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

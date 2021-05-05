using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace TDAmeritrade
{
    /// <summary>
    /// https://developer.tdameritrade.com/content/streaming-data
    /// </summary>
    public class TDRealtimeClient
    {
        TDAuthClient _auth;

        public TDRealtimeClient(TDAuthClient auth)
        {
            _auth = auth;
        }

        public async Task<T> GetQuote<T>(string symbol) where T : QuoteBase
        {
            var json = await GetQuoteJson(symbol);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            return null;
        }

        public async Task<string> GetQuoteJson(string symbol)
        {
            if (!_auth.HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            var key = HttpUtility.UrlEncode(_auth.Result.consumer_key);

            var path = $"https://api.tdameritrade.com/v1/marketdata/{symbol}/quotes?apikey={key}";

            using (var client = new HttpClient())
            {
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

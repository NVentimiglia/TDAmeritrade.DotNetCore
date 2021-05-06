using System;
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
        /// Get price history for a symbol
        /// https://developer.tdameritrade.com/price-history/apis/get/marketdata/%7Bsymbol%7D/pricehistory
        /// </summary>
        public async Task<TDPriceCandle[]> GetPriceHistory(TDPriceHistoryRequest model) 
        {
            var json = await GetPriceHistoryJson(model);
            if (!string.IsNullOrEmpty(json))
            {
                var doc = JsonDocument.Parse(json);
                var inner = doc.RootElement.GetProperty("candles").GetRawText();
                return JsonSerializer.Deserialize<TDPriceCandle[]>(inner);
            }
            return null;
        }

        /// <summary>
        /// Get price history for a symbol
        /// https://developer.tdameritrade.com/price-history/apis/get/marketdata/%7Bsymbol%7D/pricehistory
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> GetPriceHistoryJson(TDPriceHistoryRequest model)
        {
            if (!HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            var key = HttpUtility.UrlEncode(AuthResult.consumer_key);

            var builder = new UriBuilder($"https://api.tdameritrade.com/v1/marketdata/{model.symbol}/pricehistory");
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (!IsSignedIn)
            {
                query["apiKey"] = key;
            }
            query["symbol"] = model.symbol;
            query["frequencyType"] = model.frequencyType.ToString();
            query["frequency"] = model.frequency.ToString();
            if (model.endDate.HasValue)
            {
                query["endDate"] = ToEpoch(model.endDate.Value).ToString();
                query["startDate"] = ToEpoch(model.startDate.Value).ToString();
            }
            else
            {
                query["periodType"] = model.periodType.ToString();
                query["period"] = model.period.ToString();
            }
            query["needExtendedHoursData"] = model.needExtendedHoursData.ToString();
            builder.Query = query.ToString();
            string url = builder.ToString();

            using (var client = new HttpClient())
            {
                if (IsSignedIn)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthResult.access_token);
                }
                var res = await client.GetAsync(url);

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

        int ToEpoch(DateTime r)
        {
            TimeSpan t = r - new DateTime(1970, 1, 1);
            return (int)t.TotalSeconds;
        }
    }
}

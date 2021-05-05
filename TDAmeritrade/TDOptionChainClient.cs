using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace TDAmeritrade
{

    /// <summary>
    /// Get Option Chains for optionable symbols
    /// https://developer.tdameritrade.com/option-chains/apis/get/marketdata/chains
    /// </summary>
    public class TDOptionChainClient
    {
        TDAuthClient _auth;

        public TDOptionChainClient(TDAuthClient auth)
        {
            _auth = auth;
        }

        /// <summary>
        /// Get option chain for an optionable Symbol
        /// </summary>
        public async Task<OptionChain> Get(TDOptionChainRequest request) 
        {
            var json = await GetJson(request);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<OptionChain>(json);
            }
            return null;
        }

        /// <summary>
        /// Get option chain for an optionable Symbol
        /// </summary>
        public async Task<string> GetJson(TDOptionChainRequest request)
        {
            if (!_auth.HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (!_auth.IsSignedIn)
            {
                queryString.Add("apikey", _auth.Result.consumer_key);
            }
            queryString.Add("symbol", request.symbol);
            queryString.Add("contractType", request.contractType.ToString());
            queryString.Add("strikeCount", request.strikeCount.ToString());
            queryString.Add("includeQuotes", request.includeQuotes ? "FALSE" : "TRUE");
            queryString.Add("strategy", request.strategy.ToString());
            queryString.Add("interval", request.interval.ToString());
            if (request.strike.HasValue)
            {
                queryString.Add("strike", request.strike.Value.ToString());
            }
            queryString.Add("fromDate", request.fromDate.ToString("yyyy-MM-dd"));
            queryString.Add("toDate", request.toDate.ToString("yyyy-MM-dd"));
            queryString.Add("expMonth", request.expMonth);
            queryString.Add("optionType", request.optionType.ToString());

            if (request.strategy == OptionChainStrategy.ANALYTICAL)
            {
                queryString.Add("volatility", request.volatility.ToString());
                queryString.Add("underlyingPrice", request.underlyingPrice.ToString());
                queryString.Add("interestRate", request.interestRate.ToString());
                queryString.Add("daysToExpiration", request.daysToExpiration.ToString());
            }

            var q =  queryString.ToString(); 

            var path = $"https://api.tdameritrade.com/v1/marketdata/chains{q}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Result.access_token);
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

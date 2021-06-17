using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace TDAmeritrade
{
    public class TDAmeritradeClient
    {
        /// <summary>
        /// Client has valid token
        /// </summary>
        public bool IsSignedIn { get; private set; }

        /// <summary>
        /// Client has a consumer key (limited non-authenticated access)
        /// </summary>
        public bool HasConsumerKey { get; private set; }

        /// <summary>
        /// Raised on sign in / out
        /// </summary>
        public event Action<bool> OnSignedIn = delegate { };

        private ITDPersistentCache _cache;

        public TDAmeritradeClient(ITDPersistentCache cache)
        {
            _cache = cache;
        }

        public TDAmeritradeClient()
        {
            _cache = new TDUnprotectedCache();
        }

        #region Helpers  
     
        private static bool IsNullOrEmpty(string s)
        {
            return string.IsNullOrEmpty(s) || s == "{}";
        }
        #endregion

        #region Auth

        protected TDAuthResult AuthResult = new TDAuthResult();

        /// <summary>
        /// Returns sign in url
        /// https://www.reddit.com/r/algotrading/comments/c81vzq/td_ameritrade_api_access_2019_guide/
        /// https://www.reddit.com/r/algotrading/comments/914q22/successful_access_to_td_ameritrade_api/
        /// </summary>
        public string GetSignInUrl(string consumerKey, string redirectUrl = "http://localhost")
        {
            var encodedKey = HttpUtility.UrlEncode(consumerKey);
            var encodedUri = HttpUtility.UrlEncode(redirectUrl);
            var path = $"https://auth.tdameritrade.com/auth?response_type=code&redirect_uri={encodedUri}&client_id={encodedKey}%40AMER.OAUTHAP";
            return path;
        }

        /// <summary>
        /// Sign in using code from SignInUrl
        /// The token endpoint returns an access token along with an optional refresh token.
        /// https://developer.tdameritrade.com/authentication/apis/post/token-0
        /// </summary>
        /// <param name="consumerKey">OAuth User ID of your application</param>
        /// <param name="code">Required if trying to use authorization code grant</param>
        /// <param name="redirectUrl">Required if trying to use authorization code grant</param>
        /// <returns></returns>
        public async Task SignIn(string consumerKey, string code, string redirectUrl = "http://localhost")
        {
            var decoded = HttpUtility.UrlDecode(code);
            var path = "https://api.tdameritrade.com/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                var dict = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "access_type", "offline" },
                    { "client_id", $"{consumerKey}@AMER.OAUTHAP" },
                    { "redirect_uri", redirectUrl },
                    { "code", decoded }
                };

                var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(dict) };
                var res = await client.SendAsync(req);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var json = await res.Content.ReadAsStringAsync();
                        AuthResult = JsonConvert.DeserializeObject<TDAuthResult>(json);
                        AuthResult.security_code = code;
                        AuthResult.consumer_key = consumerKey;
                        AuthResult.redirect_url = redirectUrl;
                        _cache.Save("TDAmeritradeKey", JsonConvert.SerializeObject(AuthResult));
                        IsSignedIn = true;
                        HasConsumerKey = true;
                        OnSignedIn(true);
                        break;
                    default:
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }

        /// <summary>
        /// Signs in using saved refresh token
        /// </summary>
        /// <returns></returns>
        public async Task SignIn()
        {
            AuthResult = JsonConvert.DeserializeObject<TDAuthResult>(_cache.Load("TDAmeritradeKey"));

            var decoded = HttpUtility.UrlDecode(AuthResult.security_code);

            var path = "https://api.tdameritrade.com/v1/oauth2/token";

            var dict = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "access_type", "" },
                    { "client_id", $"{AuthResult.consumer_key}@AMER.OAUTHAP" },
                    { "redirect_uri", AuthResult.refresh_token },
                    { "refresh_token", AuthResult.refresh_token },
                    { "code", decoded }
                };

            var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(dict) };

            using (var client = new HttpClient())
            {
                var res = await client.SendAsync(req);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var json = await res.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<TDAuthResult>(json);
                        AuthResult.access_token = result.access_token;
                        _cache.Save("TDAmeritradeKey", JsonConvert.SerializeObject(AuthResult));
                        IsSignedIn = true;
                        HasConsumerKey = true;
                        OnSignedIn(true);
                        break;
                    default:
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }

        /// <summary>
        /// Removes security key, does not 
        /// </summary>
        public void SignOut(bool keeyConsumerKey = false, bool deleteCache = true)
        {
            AuthResult = new TDAuthResult
            {
                consumer_key = keeyConsumerKey? AuthResult.consumer_key : null
            };

            if (deleteCache)
            {
                _cache.Save("TDAmeritradeKey", JsonConvert.SerializeObject(AuthResult));
            }

            IsSignedIn = false;
            OnSignedIn(false);
        }
        #endregion

        #region Options

        /// <summary>
        /// Get option chain for an optionable Symbol
        /// https://developer.tdameritrade.com/option-chains/apis/get/marketdata/chains
        /// </summary>
        public async Task<TDOptionChain> GetOptionsChain(TDOptionChainRequest request)
        {
            var json = await GetOptionsChainJson(request);
            if (!IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<TDOptionChain>(json, new TDOptionChainConverter());
            }
            return null;
        }

        /// <summary>
        /// Get option chain for an optionable Symbol
        /// https://developer.tdameritrade.com/option-chains/apis/get/marketdata/chains
        /// </summary>
        public async Task<string> GetOptionsChainJson(TDOptionChainRequest request)
        {
            if (!HasConsumerKey)
            {
                throw (new Exception("ConsumerKey is null"));
            }

            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            if (!IsSignedIn)
            {
                queryString.Add("apikey", AuthResult.consumer_key);
            }
            queryString.Add("symbol", request.symbol);
            if (request.contractType.HasValue)
            {
                queryString.Add("contractType", request.contractType.ToString());
            }
            if (request.strikeCount.HasValue)
            {
                queryString.Add("strikeCount", request.strikeCount.ToString());
            }
            queryString.Add("includeQuotes", request.includeQuotes ? "FALSE" : "TRUE");
            if (request.interval.HasValue)
            {
                queryString.Add("interval", request.interval.ToString());
            }
            if (request.strike.HasValue)
            {
                queryString.Add("strike", request.strike.Value.ToString());
            }
            if (request.fromDate.HasValue)
            {
                queryString.Add("fromDate", request.fromDate.Value.ToString("yyyy-MM-dd"));
            }
            if (request.toDate.HasValue)
            {
                queryString.Add("toDate", request.toDate.Value.ToString("yyyy-MM-dd"));
            }
            if (!string.IsNullOrEmpty(request.expMonth))
            {
                queryString.Add("expMonth", request.expMonth);
            }
            queryString.Add("optionType", request.optionType.ToString());

            if (request.strategy == TDOptionChainStrategy.ANALYTICAL)
            {
                queryString.Add("volatility", request.volatility.ToString());
                queryString.Add("underlyingPrice", request.underlyingPrice.ToString());
                queryString.Add("interestRate", request.interestRate.ToString());
                queryString.Add("daysToExpiration", request.daysToExpiration.ToString());
            }

            var q = queryString.ToString();

            var path = $"https://api.tdameritrade.com/v1/marketdata/chains?{q}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthResult.access_token);
                var res = await client.GetAsync(path);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return await res.Content.ReadAsStringAsync();
                    default:
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }
        #endregion

        #region PriceHistory
        /// <summary>
        /// Get price history for a symbol
        /// https://developer.tdameritrade.com/price-history/apis/get/marketdata/%7Bsymbol%7D/pricehistory
        /// https://developer.tdameritrade.com/content/price-history-samples
        /// </summary>
        public async Task<TDPriceCandle[]> GetPriceHistory(TDPriceHistoryRequest model)
        {
            var json = await GetPriceHistoryJson(model);
            if (!IsNullOrEmpty(json))
            {
                var doc = JObject.Parse(json);
                var inner = doc["candles"].ToObject<TDPriceCandle[]>();
                return inner;
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
                throw (new Exception("ConsumerKey is null"));
            }

            var key = HttpUtility.UrlEncode(AuthResult.consumer_key);

            var builder = new UriBuilder($"https://api.tdameritrade.com/v1/marketdata/{model.symbol}/pricehistory");
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (!IsSignedIn)
            {
                query["apikey"] = key;
            }
            if (model.frequencyType.HasValue)
            {
                query["frequencyType"] = model.frequencyType.ToString();
                query["frequency"] = model.frequency.ToString();
            }
            if (model.endDate.HasValue)
            {
                query["endDate"] = model.endDate.Value.ToString();
                query["startDate"] = model.startDate.Value.ToString();
            }
            if(model.periodType.HasValue)
            {
                query["periodType"] = model.periodType.ToString();
                query["period"] = model.period.ToString();
            }
            if (model.needExtendedHoursData.HasValue)
            {
                query["needExtendedHoursData"] = model.needExtendedHoursData.ToString();
            }
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
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }

        #endregion

        #region Quotes

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
            if (!IsNullOrEmpty(json))
            {
                var doc = JObject.Parse(json);
                var inner = doc.First.First as JObject;
                return inner.ToObject<T>();
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
                throw (new Exception("ConsumerKey is null"));
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
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }
        #endregion

        #region UserInfo
        /// User Principal details.        
        /// </summary>
        /// <param name="fields">A comma separated String which allows one to specify additional fields to return. None of these fields are returned by default.</param>
        /// <returns></returns>
        public async Task<TDPrincipal> GetPrincipals(params TDPrincipalsFields[] fields)
        {
            var json = await GetPrincipalsJson(fields);
            if (!IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<TDPrincipal>(json);
            }
            return null;
        }

        /// <summary>
        /// User Principal details.        
        /// </summary>
        /// <param name="fields">A comma separated String which allows one to specify additional fields to return. None of these fields are returned by default.</param>
        /// <returns></returns>
        public async Task<string> GetPrincipalsJson(params TDPrincipalsFields[] fields)
        {
            if (!IsSignedIn)
            {
                throw (new Exception("Not authenticated"));
            }

            var arg = string.Join(",", fields.Select(o => o.ToString()));

            var path = $"https://api.tdameritrade.com/v1/userprincipals?fields={arg}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthResult.access_token);
                var res = await client.GetAsync(path);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return await res.Content.ReadAsStringAsync();
                    default:
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }
        #endregion

        #region Misc

        /// <summary>
        /// Retrieve market hours for specified single market
        /// </summary>
        public async Task<TDMarketHour> GetMarketHours(MarketTypes type, DateTime day)
        {
            var json = await GetMarketHoursJson(type, day);
            if (!IsNullOrEmpty(json))
            {
                var doc = JObject.Parse(json);
                return doc.First.First.First.First.ToObject<TDMarketHour>();
            }
            return null;
        }

        /// <summary>
        /// Retrieve market hours for specified single market
        /// </summary>
        public async Task<string> GetMarketHoursJson(MarketTypes type, DateTime day)
        {
            if (!IsSignedIn)
            {
                throw (new Exception("ConsumerKey is null"));
            }

            var key = HttpUtility.UrlEncode(AuthResult.consumer_key);
            var dayString = day.ToString("yyyy-MM-dd").Replace("/","-");
            string path = IsSignedIn
                ? $"https://api.tdameritrade.com/v1/marketdata/{type}/hours?date={dayString}"
                : $"https://api.tdameritrade.com/v1/marketdata/{type}/hours?apikey={key}&date={dayString}";

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
                        throw (new Exception($"{res.StatusCode} {res.ReasonPhrase}"));
                }
            }
        }
        #endregion

    }
}

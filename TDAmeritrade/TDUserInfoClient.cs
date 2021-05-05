using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace TDAmeritrade
{
    /// <summary>
    /// APIs to access user-authorized accounts and their preferences
    /// </summary>
    public class TDUserInfoClient
    {
        public enum PrincipalsFields
        {
            streamerSubscriptionKeys,
            streamerConnectionInfo,
            preferences
        }

        TDAuthClient _auth;

        public TDUserInfoClient(TDAuthClient auth)
        {
            _auth = auth;
        }

        /// <summary>
        /// User Principal details.        
        /// </summary>
        /// <param name="fields">A comma separated String which allows one to specify additional fields to return. None of these fields are returned by default.</param>
        /// <returns></returns>
        public async Task<TDPrincipal> GetPrincipals(params PrincipalsFields[] fields)
        {
            var json = await GetPrincipalsJson(fields);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<TDPrincipal>(json);
            }
            return null;
        }

        /// <summary>
        /// User Principal details.        
        /// </summary>
        /// <param name="fields">A comma separated String which allows one to specify additional fields to return. None of these fields are returned by default.</param>
        /// <returns></returns>
        public async Task<string> GetPrincipalsJson(params PrincipalsFields[] fields)
        {
            if (!_auth.HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            var arg = string.Join(',', fields.Select(o => o.ToString()));

            var key = HttpUtility.UrlEncode(_auth.Result.consumer_key);

            var path = $"https://api.tdameritrade.com/v1/userprincipals?fields={arg}";

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

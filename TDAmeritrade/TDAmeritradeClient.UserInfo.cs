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
    public partial class TDAmeritradeClient
    {        /// <summary>
        /// User Principal details.        
        /// </summary>
        /// <param name="fields">A comma separated String which allows one to specify additional fields to return. None of these fields are returned by default.</param>
        /// <returns></returns>
        public async Task<TDPrincipal> GetPrincipals(params TDPrincipalsFields[] fields)
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
        public async Task<string> GetPrincipalsJson(params TDPrincipalsFields[] fields)
        {
            if (!HasConsumerKey)
            {
                throw new Exception("Consumer key is required");
            }

            var arg = string.Join(',', fields.Select(o => o.ToString()));

            var key = HttpUtility.UrlEncode(AuthResult.consumer_key);

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
                        Console.WriteLine("Error: " + res.ReasonPhrase);
                        return null;
                }
            }
        }
    }
}

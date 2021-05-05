using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace TDAmeritrade
{

    /// <summary>
    /// The token endpoint returns an access token along with an optional refresh token.
    /// </summary>
    public class TDAuthClient
    {
        public AuthResult Result = new AuthResult();

        public bool IsSignedIn { get; private set; }
        public bool HasConsumerKey { get; private set; }

        private ITDPersistentCache _cache;

        public TDAuthClient(ITDPersistentCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Sets consumer key. Allows limited api access without authentication.
        /// </summary>
        /// <param name="consumerKey"></param>
        public void SetConsumerKey(string consumerKey)
        {
            Result.consumer_key = consumerKey;
            HasConsumerKey = true;
        }

        /// <summary>
        /// https://www.reddit.com/r/algotrading/comments/c81vzq/td_ameritrade_api_access_2019_guide/
        /// https://www.reddit.com/r/algotrading/comments/914q22/successful_access_to_td_ameritrade_api/
        /// </summary>
        public void RequestAccessToken(string consumerKey, string redirectUrl = "http://localhost")
        {
            var encodedKey = HttpUtility.UrlEncode(consumerKey);
            var encodedUri = HttpUtility.UrlEncode(redirectUrl);
            var path = $"https://auth.tdameritrade.com/auth?response_type=code&redirect_uri={encodedUri}&client_id={encodedKey}%40AMER.OAUTHAP";
            OpenBrowser(path);
        }

        /// <summary>
        /// The token endpoint returns an access token along with an optional refresh token.
        /// https://developer.tdameritrade.com/authentication/apis/post/token-0
        /// </summary>
        /// <param name="consumerKey">OAuth User ID of your application</param>
        /// <param name="code">Required if trying to use authorization code grant</param>
        /// <param name="redirectUrl">Required if trying to use authorization code grant</param>
        /// <returns></returns>
        public async Task PostAccessToken(string consumerKey, string code, string redirectUrl = "http://localhost")
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
                        var r = await res.Content.ReadAsStringAsync();
                        Console.WriteLine(r);
                        Result = JsonSerializer.Deserialize<AuthResult>(r);
                        Result.security_code = code;
                        Result.consumer_key = consumerKey;
                        Result.redirect_url = redirectUrl;
                        _cache.Save("TDAuthClient", JsonSerializer.Serialize(Result));
                        IsSignedIn = true;
                        HasConsumerKey = true;
                        break;
                    default:
                        Console.WriteLine("Error: " + res.ReasonPhrase);
                        break;
                }
            }
        }

        /// <summary>
        /// Signs in using cache authentication code
        /// </summary>
        /// <returns></returns>
        public async Task PostRefreshToken()
        {
            Result = JsonSerializer.Deserialize<AuthResult>(_cache.Load("TDAuthClient"));

            var decoded = HttpUtility.UrlDecode(Result.security_code);

            var path = "https://api.tdameritrade.com/v1/oauth2/token";

            var dict = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "access_type", "" },
                    { "client_id", $"{Result.consumer_key}@AMER.OAUTHAP" },
                    { "redirect_uri", Result.refresh_token },
                    { "refresh_token", Result.refresh_token },
                    { "code", decoded }
                };

            var req = new HttpRequestMessage(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(dict) };

            using (var client = new HttpClient())
            {
                var res = await client.SendAsync(req);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var r = await res.Content.ReadAsStringAsync();
                        Console.WriteLine(r);
                        var result = JsonSerializer.Deserialize<AuthResult>(r);
                        Result.access_token = result.access_token;
                        _cache.Save("TDAuthClient", JsonSerializer.Serialize(Result));
                        IsSignedIn = true;
                        HasConsumerKey = true;
                        break;
                    default:
                        Console.WriteLine("Error: " + res.ReasonPhrase);
                        break;
                }
            }
        }

        static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}

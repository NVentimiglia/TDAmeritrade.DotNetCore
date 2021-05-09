using System;

namespace TDAmeritrade
{
    [Serializable]
    public class TDAuthResult
    {
        public string redirect_url { get; set; }
        public string consumer_key { get; set; }
        public string security_code { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public int expires_in { get; set; }
        public int refresh_token_expires_in { get; set; }
        public string token_type { get; set; }
    }
}

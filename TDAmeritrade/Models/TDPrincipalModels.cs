using System;
using System.Collections.Generic;

namespace TDAmeritrade
{
    [Serializable]
    public class TDStreamerInfo
    {
        public string streamerBinaryUrl { get; set; }
        public string streamerSocketUrl { get; set; }
        public string token { get; set; }
        public string tokenTimestamp { get; set; }
        public string userGroup { get; set; }
        public string accessLevel { get; set; }
        public string acl { get; set; }
        public string appId { get; set; }
    }

    [Serializable]
    public class TDQuotes
    {
        public bool isNyseDelayed { get; set; }
        public bool isNasdaqDelayed { get; set; }
        public bool isOpraDelayed { get; set; }
        public bool isAmexDelayed { get; set; }
        public bool isCmeDelayed { get; set; }
        public bool isIceDelayed { get; set; }
        public bool isForexDelayed { get; set; }
    }

    [Serializable]
    public class TDKey
    {
        public string key { get; set; }
    }

    [Serializable]
    public class TDStreamerSubscriptionKeys
    {
        public List<TDKey> keys { get; set; }
    }

    [Serializable]
    public class TDPreferences
    {
        public bool expressTrading { get; set; }
        public bool directOptionsRouting { get; set; }
        public bool directEquityRouting { get; set; }
        public string defaultEquityOrderLegInstruction { get; set; }
        public string defaultEquityOrderType { get; set; }
        public string defaultEquityOrderPriceLinkType { get; set; }
        public string defaultEquityOrderDuration { get; set; }
        public string defaultEquityOrderMarketSession { get; set; }
        public int defaultEquityQuantity { get; set; }
        public string mutualFundTaxLotMethod { get; set; }
        public string optionTaxLotMethod { get; set; }
        public string equityTaxLotMethod { get; set; }
        public string defaultAdvancedToolLaunch { get; set; }
        public string authTokenTimeout { get; set; }
    }

    [Serializable]
    public class TDAuthorizations
    {
        public bool apex { get; set; }
        public bool levelTwoQuotes { get; set; }
        public bool stockTrading { get; set; }
        public bool marginTrading { get; set; }
        public bool streamingNews { get; set; }
        public string optionTradingLevel { get; set; }
        public bool streamerAccess { get; set; }
        public bool advancedMargin { get; set; }
        public bool scottradeAccount { get; set; }
    }

    [Serializable]
    public class TDAccount
    {
        public string accountId { get; set; }
        public string description { get; set; }
        public string displayName { get; set; }
        public string accountCdDomainId { get; set; }
        public string company { get; set; }
        public string segment { get; set; }
        public string surrogateIds { get; set; }
        public TDPreferences preferences { get; set; }
        public string acl { get; set; }
        public TDAuthorizations authorizations { get; set; }
    }

    [Serializable]
    public class TDPrincipal
    {
        public string authToken { get; set; }
        public string userId { get; set; }
        public string userCdDomainId { get; set; }
        public string primaryAccountId { get; set; }
        public string lastLoginTime { get; set; }
        public string tokenExpirationTime { get; set; }
        public string loginTime { get; set; }
        public string accessLevel { get; set; }
        public bool stalePassword { get; set; }
        public TDStreamerInfo streamerInfo { get; set; }
        public string professionalStatus { get; set; }
        public TDQuotes quotes { get; set; }
        public TDStreamerSubscriptionKeys streamerSubscriptionKeys { get; set; }
        public List<TDAccount> accounts { get; set; }
    }
}

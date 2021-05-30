using System;

namespace TDAmeritrade
{
    [Serializable]
    public enum MarketTypes
    {
        BOND, EQUITY, ETF, FOREX, FUTURE, FUTURE_OPTION, INDEX, INDICAT, MUTUAL_FUND, OPTION, UNKNOWN
    }

    public class TDMarketHour
    {
        public DateTime date { get; set; }
        public string marketType { get; set; }
        public bool isOpen { get; set; }
    }
}

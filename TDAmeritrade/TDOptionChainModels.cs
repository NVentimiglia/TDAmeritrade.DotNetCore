using System;
using System.Collections.Generic;

namespace TDAmeritrade
{
    public enum OptionChainTypes
    {
        ALL,
        PUT,
        CALL
    }

    public enum OptionChainStrategy
    {
        SINGLE,
        ANALYTICAL,
        COVERED,
        VERTICAL,
        CALENDAR,
        STRANGLE,
        STRADDLE,
        BUTTERFLY,
        CONDOR,
        DIAGNOL,
        COLLAR,
        ROLL
    }

    public enum OptionChainOptionTypes
    {
        /// <summary>
        /// All
        /// </summary>
        ALL,
        /// <summary>
        /// Standard
        /// </summary>
        S,
        /// <summary>
        /// NonStandard
        /// </summary>
        NS
    }

    /// <summary>
    /// ITM: In-the-money
    /// NTM: Near-the-money
    /// OTM: Out-of-the-money
    /// SAK: Strikes Above Market
    /// SBK: Strikes Below Market
    /// SNK: Strikes Near Market
    /// ALL: All Strikes
    /// </summary>
    public enum OptionChainRanges
    {
        /// ALL: All Strikes
        ALL,
        /// ITM: In-the-money
        ITM,
        /// NTM: Near-the-money
        NTM,
        /// OTM: Out-of-the-money
        OTM,
        /// SAK: Strikes Above Market
        SAK,
        /// SBK: Strikes Below Market
        SBK,
        /// SNK: Strikes Near Market
        SNK,
    }


    public class TDOptionChainRequest
    {
        /// <summary>
        /// security id
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// The number of strikes to return above and below the at-the-money price.
        /// </summary>
        public int strikeCount { get; set; }
        /// <summary>
        /// Passing a value returns a Strategy Chain
        /// </summary>
        public OptionChainStrategy strategy { get; set; }
        /// <summary>
        /// Type of contracts to return in the chai
        /// </summary>
        public OptionChainTypes contractType { get; set; }
        /// <summary>
        /// Only return expirations after this date
        /// </summary>
        public DateTime fromDate { get; set; }
        /// <summary>
        /// Only return expirations before this date
        /// </summary>
        public DateTime toDate { get; set; }

        /// <summary>
        /// Strike interval for spread strategy chains
        /// </summary>
        public double interval {get;set; }
        /// <summary>
        /// Provide a strike price to return options only at that strike price.
        /// </summary>
        public double? strike { get; set; }
        /// <summary>
        /// Returns options for the given range
        /// </summary>
        public OptionChainRanges range { get; set; }
        /// <summary>
        /// Volatility to use in calculations. ANALYTICAL  only.
        /// </summary>
        public double volatility { get; set; }
        /// <summary>
        /// Underlying price to use in calculations. ANALYTICAL  only.
        /// </summary>
        public double underlyingPrice { get; set; }
        /// <summary>
        /// Interest rate to use in calculations. ANALYTICAL  only.
        /// </summary>
        public double interestRate { get; set; }
        /// <summary>
        /// Days to expiration to use in calculations. Applies only to ANALYTICAL strategy chains
        /// </summary>
        public int daysToExpiration { get; set; }
        /// <summary>
        /// Return only options expiring in the specified month
        /// </summary>
        public string expMonth { get; set; } = "ALL";
        /// <summary>
        /// Include quotes for options in the option chain. Can be TRUE or FALSE. Default is FALSE.
        /// </summary>
        public bool includeQuotes { get; set; }
        /// <summary>
        /// Type of contracts to return
        /// </summary>
        public OptionChainOptionTypes optionType { get; set; }
    }

    public class OptionChain
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public Underlying underlying { get; set; }
        public string strategy { get; set; }
        public int interval { get; set; }
        public bool isDelayed { get; set; }
        public bool isIndex { get; set; }
        public int daysToExpiration { get; set; }
        public int interestRate { get; set; }
        public int underlyingPrice { get; set; }
        public int volatility { get; set; }
        public string callExpDateMap { get; set; }
        public string putExpDateMap { get; set; }
    }

    public class OptionDeliverablesList
    {
        public string symbol { get; set; }
        public string assetType { get; set; }
        public string deliverableUnits { get; set; }
        public string currencyType { get; set; }
    }

    public class Option
    {
        public string putCall { get; set; }
        public string symbol { get; set; }
        public string description { get; set; }
        public string exchangeName { get; set; }
        public int bidPrice { get; set; }
        public int askPrice { get; set; }
        public int lastPrice { get; set; }
        public int markPrice { get; set; }
        public int bidSize { get; set; }
        public int askSize { get; set; }
        public int lastSize { get; set; }
        public int highPrice { get; set; }
        public int lowPrice { get; set; }
        public int openPrice { get; set; }
        public int closePrice { get; set; }
        public int totalVolume { get; set; }
        public int quoteTimeInLong { get; set; }
        public int tradeTimeInLong { get; set; }
        public int netChange { get; set; }
        public int volatility { get; set; }
        public int delta { get; set; }
        public int gamma { get; set; }
        public int theta { get; set; }
        public int vega { get; set; }
        public int rho { get; set; }
        public int timeValue { get; set; }
        public int openInterest { get; set; }
        public bool isInTheMoney { get; set; }
        public int theoreticalOptionValue { get; set; }
        public int theoreticalVolatility { get; set; }
        public bool isMini { get; set; }
        public bool isNonStandard { get; set; }
        public List<OptionDeliverablesList> optionDeliverablesList { get; set; }
        public int strikePrice { get; set; }
        public string expirationDate { get; set; }
        public string expirationType { get; set; }
        public int multiplier { get; set; }
        public string settlementType { get; set; }
        public string deliverableNote { get; set; }
        public bool isIndexOption { get; set; }
        public int percentChange { get; set; }
        public int markChange { get; set; }
        public int markPercentChange { get; set; }
    }

    public class Underlying
    {
        public int ask { get; set; }
        public int askSize { get; set; }
        public int bid { get; set; }
        public int bidSize { get; set; }
        public int change { get; set; }
        public int close { get; set; }
        public bool delayed { get; set; }
        public string description { get; set; }
        public string exchangeName { get; set; }
        public int fiftyTwoWeekHigh { get; set; }
        public int fiftyTwoWeekLow { get; set; }
        public int highPrice { get; set; }
        public int last { get; set; }
        public int lowPrice { get; set; }
        public int mark { get; set; }
        public int markChange { get; set; }
        public int markPercentChange { get; set; }
        public int openPrice { get; set; }
        public int percentChange { get; set; }
        public int quoteTime { get; set; }
        public string symbol { get; set; }
        public int totalVolume { get; set; }
        public int tradeTime { get; set; }
    }


    public class ExpirationDate
    {
        public string date { get; set; }
    }

    public class OptionDeliverables
    {
        public string symbol { get; set; }
        public string assetType { get; set; }
        public string deliverableUnits { get; set; }
        public string currencyType { get; set; }
    }


}

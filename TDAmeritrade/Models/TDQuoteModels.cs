using System;

namespace TDAmeritrade
{
    [Serializable]
    public abstract class TDQuoteBase
    {
        public string symbol { get; set; }
        public string description { get; set; }
        public string exchangeName { get; set; }
        public string securityStatus { get; set; }
    }

    [Serializable]
    public class MarketQuoteBase : TDQuoteBase
    {
        public double bidPrice { get; set; }
        public int bidSize { get; set; }
        public double askPrice { get; set; }
        public int askSize { get; set; }
        public double lastPrice { get; set; }
        public int lastSize { get; set; }
        public double openPrice { get; set; }
        public double highPrice { get; set; }
        public double lowPrice { get; set; }
        public double closePrice { get; set; }
        public double netChange { get; set; }
        public long totalVolume { get; set; }
        public long quoteTimeInLong { get; set; }
        public long tradeTimeInLong { get; set; }
        public double mark { get; set; }
        public string exchange { get; set; }
        public double volatility { get; set; }
    }

    [Serializable]
    public class TDEquityQuote : MarketQuoteBase
    {
        public string bidId { get; set; }
        public string askId { get; set; }
        public string lastId { get; set; }
        public bool marginable { get; set; }
        public bool shortable { get; set; }
        public int digits { get; set; }
        public double _52WkHigh { get; set; }
        public double _52WkLow { get; set; }
        public double peRatio { get; set; }
        public double divAmount { get; set; }
        public double divYield { get; set; }
        public string divDate { get; set; }
        public double regularMarketLastPrice { get; set; }
        public int regularMarketLastSize { get; set; }
        public double regularMarketNetChange { get; set; }
        public long regularMarketTradeTimeInLong { get; set; }
    }

    [Serializable]
    public class TDFundQuote : TDQuoteBase
    {
        public double closePrice { get; set; }
        public double netChange { get; set; }
        public int totalVolume { get; set; }
        public long tradeTimeInLong { get; set; }
        public string exchange { get; set; }
        public int digits { get; set; }
        public double _52WkHigh { get; set; }
        public double _52WkLow { get; set; }
        public double nAV { get; set; }
        public double peRatio { get; set; }
        public double divAmount { get; set; }
        public double divYield { get; set; }
        public string divDate { get; set; }
    }

    [Serializable]
    public class FutureQuote : TDQuoteBase
    {
        public double bidPriceInDouble { get; set; }
        public double askPriceInDouble { get; set; }
        public double lastPriceInDouble { get; set; }
        public string bidId { get; set; }
        public string askId { get; set; }
        public double highPriceInDouble { get; set; }
        public double lowPriceInDouble { get; set; }
        public double closePriceInDouble { get; set; }
        public string exchange { get; set; }
        public string lastId { get; set; }
        public double openPriceInDouble { get; set; }
        public double changeInDouble { get; set; }
        public double futurePercentChange { get; set; }
        public int openInterest { get; set; }
        public int mark { get; set; }
        public int tick { get; set; }
        public int tickAmount { get; set; }
        public string product { get; set; }
        public string futurePriceFormat { get; set; }
        public string futureTradingHours { get; set; }
        public bool futureIsTradable { get; set; }
        public int futureMultiplier { get; set; }
        public bool futureIsActive { get; set; }
        public double futureSettlementPrice { get; set; }
        public string futureActiveSymbol { get; set; }
        public string futureExpirationDate { get; set; }
    }

    [Serializable]
    public class FutureOptionsQuote : TDQuoteBase
    {
        public double bidPriceInDouble { get; set; }
        public double askPriceInDouble { get; set; }
        public double lastPriceInDouble { get; set; }
        public double highPriceInDouble { get; set; }
        public double lowPriceInDouble { get; set; }
        public double closePriceInDouble { get; set; }
        public double openPriceInDouble { get; set; }
        public double netChangeInDouble { get; set; }
        public int openInterest { get; set; }
        public int volatility { get; set; }
        public int moneyIntrinsicValueInDouble { get; set; }
        public double multiplierInDouble { get; set; }
        public int digits { get; set; }
        public double strikePriceInDouble { get; set; }
        public string contractType { get; set; }
        public string underlying { get; set; }
        public double timeValueInDouble { get; set; }
        public double deltaInDouble { get; set; }
        public double gammaInDouble { get; set; }
        public double thetaInDouble { get; set; }
        public double vegaInDouble { get; set; }
        public double rhoInDouble { get; set; }
        public int mark { get; set; }
        public int tick { get; set; }
        public int tickAmount { get; set; }
        public bool futureIsTradable { get; set; }
        public string futureTradingHours { get; set; }
        public double futurePercentChange { get; set; }
        public bool futureIsActive { get; set; }
        public int futureExpirationDate { get; set; }
        public string expirationType { get; set; }
        public string exerciseType { get; set; }
        public bool inTheMoney { get; set; }
    }

    /// <summary>
    ///  SPY,$SPX.X, QQQ,$NDX.X, IWM,$RUT.X, IYY,$DJI2MN Vol indexes $VIX.X,$VXX.X,$VXN.X,$RVX.X
    /// </summary>
    [Serializable]
    public class TDIndexQuote : TDQuoteBase
    {
        public double lastPrice { get; set; }
        public double openPrice { get; set; }
        public double highPrice { get; set; }
        public double lowPrice { get; set; }
        public double closePrice { get; set; }
        public double netChange { get; set; }
        public long totalVolume { get; set; }
        public long tradeTimeInLong { get; set; }
        public string exchange { get; set; }
        public int digits { get; set; }
        public double _52WkHigh { get; set; }
        public double _52WkLow { get; set; }
    }
    [Serializable]
    public class TDForexQuote : TDQuoteBase
    {
        public double bidPriceInDouble { get; set; }
        public double askPriceInDouble { get; set; }
        public double lastPriceInDouble { get; set; }
        public double highPriceInDouble { get; set; }
        public double lowPriceInDouble { get; set; }
        public double closePriceInDouble { get; set; }
        public string exchange { get; set; }
        public double openPriceInDouble { get; set; }
        public double changeInDouble { get; set; }
        public int percentChange { get; set; }
        public int digits { get; set; }
        public int tick { get; set; }
        public int tickAmount { get; set; }
        public string product { get; set; }
        public string tradingHours { get; set; }
        public bool isTradable { get; set; }
        public string marketMaker { get; set; }
        public double _52WkHighInDouble { get; set; }
        public double _52WkLowInDouble { get; set; }
        public int mark { get; set; }
    }

    [Serializable]
    public class TDOptionQuote : MarketQuoteBase
    {
        public int openInterest { get; set; }
        public double moneyIntrinsicValue { get; set; }
        public double multiplier { get; set; }
        public double strikePrice { get; set; }
        public string contractType { get; set; }
        public string underlying { get; set; }
        public double timeValue { get; set; }
        public string deliverables { get; set; }
        public double delta { get; set; }
        public double gamma { get; set; }
        public double theta { get; set; }
        public double vega { get; set; }
        public double rho { get; set; }
        public double theoreticalOptionValue { get; set; }
        public double underlyingPrice { get; set; }
        public string uvExpirationType { get; set; }
        public string settlementType { get; set; }
    }

}

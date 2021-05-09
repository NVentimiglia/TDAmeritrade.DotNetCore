using System;

namespace TDAmeritrade
{
    [Serializable]
    public struct TDHeartbeat
    {
        /// <summary>
        /// UNIX
        /// </summary>        
        public long timestamp { get; set; }
    }

    [Serializable]
    public enum TDBookOptions
    {
        LISTED_BOOK,
        NASDAQ_BOOK,
        OPTIONS_BOOK,
        FUTURES_BOOK,
        FOREX_BOOK,
        FUTURES_OPTIONS_BOOK,
    }

    [Serializable]
    public struct TDBookSignal
    {
        /// <summary>
        /// UNIX
        /// </summary>        
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// 1
        /// </summary>
        public long booktime { get; set; }
        /// <summary>
        /// 2
        /// </summary>
        public double bids { get; set; }
        /// <summary>
        /// 3
        /// </summary>
        public double asks { get; set; }

    }

    [Serializable]
    public struct TDQuoteSignal
    {
        /// <summary>
        /// UNIX
        /// </summary>        
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }


        /// <summary>
        /// 1 Current Best Bid Price
        /// </summary>
        public double bidprice { get; set; }
        /// <summary>
        /// 2 Current Best Ask Price
        /// </summary>
        public double askprice { get; set; }
        /// 4 Number of shares for bid
        /// </summary>
        public double bidsize { get; set; }
        /// <summary>
        /// 5 Number of shares for ask
        /// </summary>
        public double asksize { get; set; }


        /// <summary>
        /// 3 Price at which the last trade was matched
        /// </summary>
        public double lastprice { get; set; }
        /// <summary>
        /// 9 Number of shares traded with last trade
        /// </summary>
        public double lastsize { get; set; }


        /// <summary>
        /// <summary>
        /// 8 Aggregated shares traded throughout the day, including pre/post market hours.
        /// </summary>
        public long totalvolume { get; set; }
        /// <summary>
        /// 28 Previous day’s opening price
        /// </summary>
        public double openprice { get; set; }
        /// <summary>
        /// 15 Previous day’s closing price
        /// </summary>
        public double closeprice { get; set; }
        /// <summary>
        /// 13 Day’s low trade price
        /// </summary>
        public double lowprice { get; set; }
        /// <summary>
        /// 12 Day’s high trade price
        /// </summary>
        public double highprice { get; set; }

        /// <summary>
        /// 10 Trade time of the last trade
        /// </summary>
        public long tradetime { get; set; }
        /// <summary>
        /// 11 Quote time of the last trade
        /// </summary>
        public long quotetime { get; set; }
        /// <summary>
        /// 7 Exchange with the best bid
        /// </summary>
        public char bidid { get; set; }
        /// <summary>
        /// 6 Exchange with the best ask
        /// </summary>
        public char askid { get; set; }
        /// <summary>
        /// 14 Indicates Up or Downtick(NASDAQ NMS & Small Cap)
        /// </summary>
        public char bidtick { get; set; }
        /// <summary>
        /// 24 Option Risk/Volatility Measurement
        /// </summary>
        public double volatility { get; set; }
    }

    [Serializable]
    public struct TDTimeSaleSignal
    {
        /// <summary>
        /// UNIX
        /// </summary>
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }

        /// <summary>
        /// order
        /// </summary>
        public long sequence { get; set; }

        /// <summary>
        /// 1 Trade time of the last trade
        /// </summary>
        public long tradetime { get; set; }
        /// <summary>
        /// 2 Price at which the last trade was matched
        /// </summary>
        public double lastprice { get; set; }
        /// <summary>
        /// 3 Number of shares traded with last trade
        /// </summary>
        public double lastsize { get; set; }
        /// <summary>
        /// 4 Number of Number of shares for bid
        /// </summary>
        public long lastsequence { get; set; }
    }

    [Serializable]
    public struct TDChartSignal
    {
        /// <summary>
        /// UNIX
        /// </summary>
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }


        /// <summary>
        /// 1 Opening price for the minute
        /// </summary>
        public double openprice { get; set; }
        /// <summary>
        /// 2 Highest price for the minute
        /// </summary>
        public double highprice { get; set; }
        /// <summary>
        /// 3 Chart’s lowest price for the minute
        /// </summary>
        public double lowprice { get; set; }
        /// <summary>
        /// 4 Closing price for the minute
        /// </summary>
        public double closeprice { get; set; }
        /// <summary>
        /// 5 Total volume for the minute
        /// </summary>
        public double volume { get; set; }
        /// <summary>
        /// 6 Identifies the candle minute
        /// </summary>
        public long sequence { get; set; }
        /// <summary>
        /// 7 Milliseconds since Epoch
        /// </summary>
        public long charttime { get; set; }
        /// <summary>
        /// 8 Not useful
        /// </summary>
        public int chartday { get; set; }
    }

    [Serializable]
    public enum TDChartSubs
    {
        CHART_EQUITY,
        CHART_OPTIONS,
        CHART_FUTURES,
    }

    [Serializable]
    public enum TDTimeSaleServices
    {
        TIMESALE_EQUITY,
        TIMESALE_FOREX,
        TIMESALE_FUTURES,
        TIMESALE_OPTIONS,
    }

    [Serializable]
    public enum TDQOSLevels
    {
        /// <summary>
        /// 500ms
        /// </summary>
        EXPRESS,
        /// <summary>
        /// 750ms
        /// </summary>
        REALTIME,
        /// <summary>
        /// 1000ms
        /// </summary>
        FAST,
        /// <summary>
        /// 1500ms
        /// </summary>
        MODERATE,
        /// <summary>
        /// 5000ms
        /// </summary>
        DELAYED,
        /// <summary>
        /// 3000ms
        /// </summary>
        SLOW,
    }

    [Serializable]
    public class TDRealtimeRequest
    {
        public string service { get; set; }
        public string command { get; set; }
        public int requestid { get; set; }
        public string account { get; set; }
        public string source { get; set; }
        public object parameters { get; set; }
    }

    [Serializable]
    public class TDRealtimeRequestContainer
    {
        public TDRealtimeRequest[] requests { get; set; }
    }

    [Serializable]
    public class TDRealtimeResponseContainer
    {
        public TDRealtimeResponse[] response { get; set; }
    }

    [Serializable]
    public class TDRealtimeResponse
    {
        public string service { get; set; }
        public string requestid { get; set; }
        public string command { get; set; }
        public long timestamp { get; set; }
        public TDRealtimeContent content { get; set; }
    }

    [Serializable]
    public class TDRealtimeContent
    {
        public int code { get; set; }
        public string msg { get; set; }
    }
}

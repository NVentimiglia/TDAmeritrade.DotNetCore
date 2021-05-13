using FlatSharp.Attributes;
using Newtonsoft.Json;
using System;

namespace TDAmeritrade
{
    /// <summary>
    /// common interface for all realtime stream signals
    /// </summary>
    public interface ISignal
    {
        /// <summary>
        /// UNIX
        /// </summary>    
        public long timestamp { get; set; }
        /// <summary>
        /// Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }
    }

    ///
    public enum SignalTypes
    {
        NA,
        HEARTBEAT,
        BOOK,
        QUOTE,
        TIMESALE,
        CHART,

    }

    [Serializable]
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(0)]
    public class TDHeartbeatSignal
    {
        /// <summary>
        /// UNIX
        /// </summary>     
        [FlatBufferItem(0)]
        public long timestamp { get; set; }
    }

    [Serializable]
    [FlatBufferEnum(typeof(byte))]
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
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(1)]
    public class TDBookSignal : ISignal
    {
        /// <summary>
        /// UNIX
        /// </summary>        
        [FlatBufferItem(0)]
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        [FlatBufferItem(1)]
        public string symbol { get; set; }
        /// <summary>
        /// Book source
        /// </summary>
        [FlatBufferItem(2)]
        public TDBookOptions id { get; set; }
        /// <summary>
        /// 2 bids
        /// </summary>
        [FlatBufferItem(3)]
        public TDBookLevel[] bids { get; set; }
        /// <summary>
        /// 3 asks
        /// </summary>
        [FlatBufferItem(4)]
        public TDBookLevel[] asks { get; set; }
    }

    [Serializable]
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(2)]
    public class TDBookLevel
    {
        /// <summary>
        /// 0 this price level
        /// </summary>
        [JsonProperty("0")]
        [FlatBufferItem(0)]
        public double price { get; set; }
        /// <summary>
        /// 2 total volume at this level
        /// </summary>
        [JsonProperty("1")]
        [FlatBufferItem(1)]
        public double quantity { get; set; }
    }

    [Serializable]
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(3)]
    public class TDQuoteSignal : ISignal
    {
        /// <summary>
        /// UNIX
        /// </summary>    
        [FlatBufferItem(0)]
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        [FlatBufferItem(1)]
        public string symbol { get; set; }


        /// <summary>
        /// 1 Current Best Bid Price
        /// </summary>
        [FlatBufferItem(2)]
        public double bidprice { get; set; }
        /// <summary>
        /// 2 Current Best Ask Price
        /// </summary>
        [FlatBufferItem(3)]
        public double askprice { get; set; }
        /// 4 Number of shares for bid
        /// </summary>
        [FlatBufferItem(4)]
        public double bidsize { get; set; }
        /// <summary>
        /// 5 Number of shares for ask
        /// </summary>
        [FlatBufferItem(5)]
        public double asksize { get; set; }


        /// <summary>
        /// 3 Price at which the last trade was matched
        /// </summary>
        [FlatBufferItem(6)]
        public double lastprice { get; set; }
        /// <summary>
        /// 9 Number of shares traded with last trade
        /// </summary>
        [FlatBufferItem(7)]
        public double lastsize { get; set; }


        /// <summary>
        /// <summary>
        /// 8 Aggregated shares traded throughout the day, including pre/post market hours.
        /// </summary>
        [FlatBufferItem(8)]
        public long totalvolume { get; set; }
        /// <summary>
        /// 28 Previous day’s opening price
        /// </summary>
        [FlatBufferItem(9)]
        public double openprice { get; set; }
        /// <summary>
        /// 15 Previous day’s closing price
        /// </summary>
        [FlatBufferItem(10)]
        public double closeprice { get; set; }
        /// <summary>
        /// 13 Day’s low trade price
        /// </summary>
        [FlatBufferItem(11)]
        public double lowprice { get; set; }
        /// <summary>
        /// 12 Day’s high trade price
        /// </summary>
        [FlatBufferItem(12)]
        public double highprice { get; set; }

        /// <summary>
        /// 10 Trade time of the last trade
        /// </summary>
        [FlatBufferItem(13)]
        public long tradetime { get; set; }
        /// <summary>
        /// 11 Quote time of the last trade
        /// </summary>
        [FlatBufferItem(14)]
        public long quotetime { get; set; }
        /// <summary>
        /// 7 Exchange with the best bid
        /// </summary>
        [FlatBufferItem(15)]
        public char bidid { get; set; }
        /// <summary>
        /// 6 Exchange with the best ask
        /// </summary>
        [FlatBufferItem(16)]
        public char askid { get; set; }
        /// <summary>
        /// 14 Indicates Up or Downtick(NASDAQ NMS & Small Cap)
        /// </summary>
        [FlatBufferItem(17)]
        public char bidtick { get; set; }
        /// <summary>
        /// 24 Option Risk/Volatility Measurement
        /// </summary>
        [FlatBufferItem(18)]
        public double volatility { get; set; }
    }

    [Serializable]
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(4)]
    public class TDTimeSaleSignal : ISignal
    {
        /// <summary>
        /// UNIX
        /// </summary>
        [FlatBufferItem(0)]
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        [FlatBufferItem(1)]
        public string symbol { get; set; }

        /// <summary>
        /// order
        /// </summary>
        [FlatBufferItem(2)]
        public long sequence { get; set; }

        /// <summary>
        /// 1 Trade time of the last trade
        /// </summary>
        [FlatBufferItem(3)]
        public long tradetime { get; set; }
        /// <summary>
        /// 2 Price at which the last trade was matched
        /// </summary>
        [FlatBufferItem(4)]
        public double lastprice { get; set; }
        /// <summary>
        /// 3 Number of shares traded with last trade
        /// </summary>
        [FlatBufferItem(5)]
        public double lastsize { get; set; }
        /// <summary>
        /// 4 Number of Number of shares for bid
        /// </summary>
        [FlatBufferItem(6)]
        public long lastsequence { get; set; }
    }

    [Serializable]
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(5)]
    public class TDChartSignal : ISignal
    {
        /// <summary>
        /// UNIX
        /// </summary>
        [FlatBufferItem(0)]
        public long timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        [FlatBufferItem(1)]
        public string symbol { get; set; }


        /// <summary>
        /// 1 Opening price for the minute
        /// </summary>
        [FlatBufferItem(2)]
        public double openprice { get; set; }
        /// <summary>
        /// 2 Highest price for the minute
        /// </summary>
        [FlatBufferItem(3)]
        public double highprice { get; set; }
        /// <summary>
        /// 3 Chart’s lowest price for the minute
        /// </summary>
        [FlatBufferItem(4)]
        public double lowprice { get; set; }
        /// <summary>
        /// 4 Closing price for the minute
        /// </summary>
        [FlatBufferItem(5)]
        public double closeprice { get; set; }
        /// <summary>
        /// 5 Total volume for the minute
        /// </summary>
        [FlatBufferItem(6)]
        public double volume { get; set; }
        /// <summary>
        /// 6 Identifies the candle minute
        /// </summary>
        [FlatBufferItem(7)]
        public long sequence { get; set; }
        /// <summary>
        /// 7 Milliseconds since Epoch
        /// </summary>
        [FlatBufferItem(8)]
        public long charttime { get; set; }
        /// <summary>
        /// 8 Not useful
        /// </summary>
        [FlatBufferItem(9)]
        public int chartday { get; set; }
    }

    [Serializable]
    [FlatBufferEnum(typeof(byte))]
    public enum TDChartSubs
    {
        CHART_EQUITY,
        CHART_OPTIONS,
        CHART_FUTURES,
    }

    [Serializable]
    [FlatBufferEnum(typeof(byte))]
    public enum TDTimeSaleServices
    {
        TIMESALE_EQUITY,
        TIMESALE_FOREX,
        TIMESALE_FUTURES,
        TIMESALE_OPTIONS,
    }

    [Serializable]
    [FlatBufferEnum(typeof(byte))]
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
    [FlatBufferTable]
    [FlatBufferHeaderAttribute(6)]
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

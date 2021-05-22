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
        public double timestamp { get; set; }

        /// <summary>
        /// Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }

        public DateTime TimeStamp { get; }

    }
    public enum TDSignalTypes
    {
        NA,
        HEARTBEAT,
        CHART,
        QUOTE,
        TIMESALE,
        BOOK
    }

    [Serializable]
    public struct TDHeartbeatSignal : IBitModel
    {
        /// <summary>
        /// UNIX
        /// </summary> 
        public double timestamp { get; set; }

        public void Parse(BitSerializer stream)
        {
            timestamp = stream.Parse(timestamp);
        }
        public DateTime TimeStamp
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(timestamp);
            }
        }
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
    public struct TDBookSignal : ISignal, IBitModel
    {
        /// <summary>
        /// UNIX
        /// </summary>  
        public double timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// Book source
        /// </summary>
        public TDBookOptions id;
        /// <summary>
        /// 2 bids
        /// </summary>
        public TDBookLevel[] bids;
        /// <summary>
        /// 3 asks
        /// </summary>
        public TDBookLevel[] asks;

        public void Parse(BitSerializer stream)
        {
            timestamp = stream.Parse(timestamp);
            symbol = stream.Parse(symbol);
            id = (TDBookOptions)stream.Parse((byte)id);
            stream.Parse(ref bids);
            stream.Parse(ref asks);
        }

        public DateTime TimeStamp
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(timestamp);
            }
        }
    }

    [Serializable]
    public struct TDBookLevel : IBitModel
    {
        /// <summary>
        /// 0 this price level
        /// </summary>
        [JsonProperty("0")]
        public double price;
        /// <summary>
        /// 2 total volume at this level
        /// </summary>
        [JsonProperty("1")]
        public double quantity;

        public void Parse(BitSerializer stream)
        {
            stream.Parse(ref price);
            stream.Parse(ref quantity);
        }
    }

    [Serializable]
    public struct TDQuoteSignal : ISignal, IBitModel
    {
        /// <summary>
        /// UNIX
        /// </summary>  
        public double timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }


        /// <summary>
        /// 1 Current Best Bid Price
        /// </summary>
        public double bidprice;
        /// <summary>
        /// 2 Current Best Ask Price
        /// </summary>
        public double askprice;
        /// 4 Number of shares for bid
        /// </summary>
        public double bidsize;
        /// <summary>
        /// 5 Number of shares for ask
        /// </summary>
        public double asksize;

        /// <summary>
        /// 3 Price at which the last trade was matched
        /// </summary>
        public double lastprice;
        /// <summary>
        /// 9 Number of shares traded with last trade
        /// </summary>
        public double lastsize;

        /// <summary>
        /// <summary>
        /// 8 Aggregated shares traded throughout the day, including pre/post market hours.
        /// </summary>
        public long totalvolume;
        /// <summary>
        /// 28 Previous day’s opening price
        /// </summary>
        public double openprice;
        /// <summary>
        /// 15 Previous day’s closing price
        /// </summary>
        public double closeprice;
        /// <summary>
        /// 13 Day’s low trade price
        /// </summary>
        public double lowprice;
        /// <summary>
        /// 12 Day’s high trade price
        /// </summary>
        public double highprice;

        /// <summary>
        /// 10 Trade time of the last trade
        /// </summary>
        public double tradetime;
        /// <summary>
        /// 11 Quote time of the last trade
        /// </summary>
        public double quotetime;
        /// <summary>
        /// 7 Exchange with the best bid
        /// </summary>
        public char bidid;
        /// <summary>
        /// 6 Exchange with the best ask
        /// </summary>
        public char askid;
        /// <summary>
        /// 14 Indicates Up or Downtick(NASDAQ NMS & Small Cap)
        /// </summary>
        public char bidtick;
        /// <summary>
        /// 24 Option Risk/Volatility Measurement
        /// </summary>
        public double volatility;

        public void Parse(BitSerializer stream)
        {
            timestamp = stream.Parse(timestamp);
            symbol = stream.Parse(symbol);
            stream.Parse(ref bidprice);
            stream.Parse(ref askprice);
            stream.Parse(ref bidsize);
            stream.Parse(ref asksize);

            stream.Parse(ref lastprice);
            stream.Parse(ref lastsize);

            stream.Parse(ref totalvolume);
            stream.Parse(ref openprice);
            stream.Parse(ref closeprice);
            stream.Parse(ref lowprice);
            stream.Parse(ref highprice);


            stream.Parse(ref tradetime);
            stream.Parse(ref quotetime);
            stream.Parse(ref bidid);
            stream.Parse(ref askid);
            stream.Parse(ref bidtick);
            stream.Parse(ref volatility);
        }
        public DateTime TimeStamp
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(timestamp);
            }
        }
        public DateTime QuoteTime
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(quotetime);
            }
        }
        public DateTime TradeTime
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(tradetime);
            }
        }
    }

    [Serializable]
    public struct TDTimeSaleSignal : ISignal, IBitModel
    {
        /// <summary>
        /// UNIX
        /// </summary>
        public double timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }

        /// <summary>
        /// order
        /// </summary>
        public long sequence;

        /// <summary>
        /// 1 Trade time of the last trade
        /// </summary>
        public double tradetime;
        /// <summary>
        /// 2 Price at which the last trade was matched
        /// </summary>
        public double lastprice;
        /// <summary>
        /// 3 Number of shares traded with last trade
        /// </summary>
        public double lastsize;
        /// <summary>
        /// 4 Number of Number of shares for bid
        /// </summary>
        public long lastsequence;

        public void Parse(BitSerializer stream)
        {
            timestamp = stream.Parse(timestamp);
            symbol = stream.Parse(symbol);
            stream.Parse(ref sequence);
            stream.Parse(ref tradetime);
            stream.Parse(ref lastprice);
            stream.Parse(ref lastsize);
            stream.Parse(ref lastsequence);
        }
        public DateTime TimeStamp
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(timestamp);
            }
        }
        public DateTime TradeTime
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(tradetime);
            }
        }
    }

    [Serializable]
    public struct TDChartSignal : ISignal, IBitModel
    {
        /// <summary>
        /// UNIX
        /// </summary>
        public double timestamp { get; set; }
        /// <summary>
        /// 0 Ticker symbol in upper case. 
        /// </summary>
        public string symbol { get; set; }


        /// <summary>
        /// 1 Opening price for the minute
        /// </summary>
        public double openprice;
        /// <summary>
        /// 2 Highest price for the minute
        /// </summary>
        public double highprice;
        /// <summary>
        /// 3 Chart’s lowest price for the minute
        /// </summary>
        public double lowprice;
        /// <summary>
        /// 4 Closing price for the minute
        /// </summary>
        public double closeprice;
        /// <summary>
        /// 5 Total volume for the minute
        /// </summary>
        public double volume;
        /// <summary>
        /// 6 Identifies the candle minute
        /// </summary>
        public long sequence;
        /// <summary>
        /// 7 Milliseconds since Epoch
        /// </summary>
        public long charttime;
        /// <summary>
        /// 8 Not useful
        /// </summary>
        public int chartday;

        public void Parse(BitSerializer stream)
        {
            timestamp = stream.Parse(timestamp);
            symbol = stream.Parse(symbol);
            stream.Parse(ref openprice);
            stream.Parse(ref highprice);
            stream.Parse(ref lowprice);
            stream.Parse(ref closeprice);
            stream.Parse(ref volume);
            stream.Parse(ref sequence);
            stream.Parse(ref charttime);
            stream.Parse(ref chartday);
        }
        public DateTime TimeStamp
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(timestamp);
            }
        }

        public DateTime ChartTime
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(charttime);
            }
        }
        public int ChartIndex
        {
            get
            {
                return TDHelpers.ToCandleIndex(ChartTime, 1);
            }
        }
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
        public double timestamp { get; set; }
        public TDRealtimeContent content { get; set; }
        public DateTime TimeStamp
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(timestamp);
            }
        }
    }

    [Serializable]
    public class TDRealtimeContent
    {
        public int code { get; set; }
        public string msg { get; set; }
    }
}

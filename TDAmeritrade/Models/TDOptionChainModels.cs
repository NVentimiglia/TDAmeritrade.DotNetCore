using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TDAmeritrade
{
    [Serializable]
    public enum TDOptionChainTypes
    {
        ALL,
        PUT,
        CALL
    }

    [Serializable]
    public enum TDOptionChainStrategy
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

    [Serializable]
    public enum TDOptionChainOptionTypes
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
    [Serializable]
    public enum TDOptionChainRanges
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

    [Serializable]
    public class TDOptionChainRequest
    {
        /// <summary>
        /// security id
        /// </summary>
        public string symbol { get; set; }
        /// <summary>
        /// The number of strikes to return above and below the at-the-money price.
        /// </summary>
        public int? strikeCount { get; set; }
        /// <summary>
        /// Passing a value returns a Strategy Chain
        /// </summary>
        public TDOptionChainStrategy strategy { get; set; }
        /// <summary>
        /// Type of contracts to return in the chai
        /// </summary>
        public TDOptionChainTypes? contractType { get; set; }
        /// <summary>
        /// Only return expirations after this date
        /// </summary>
        public DateTime? fromDate { get; set; }
        /// <summary>
        /// Only return expirations before this date
        /// </summary>
        public DateTime? toDate { get; set; }

        /// <summary>
        /// Strike interval for spread strategy chains
        /// </summary>
        public double? interval {get;set; }
        /// <summary>
        /// Provide a strike price to return options only at that strike price.
        /// </summary>
        public double? strike { get; set; }
        /// <summary>
        /// Returns options for the given range
        /// </summary>
        public TDOptionChainRanges range { get; set; }
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
        public int? daysToExpiration { get; set; }
        /// <summary>
        /// Return only options expiring in the specified month
        /// </summary>
        public string expMonth { get; set; }
        /// <summary>
        /// Include quotes for options in the option chain. Can be TRUE or FALSE. Default is FALSE.
        /// </summary>
        public bool includeQuotes { get; set; }
        /// <summary>
        /// Type of contracts to return
        /// </summary>
        public TDOptionChainOptionTypes optionType { get; set; }
    }

    [Serializable]
    public class TDOptionChain
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public TDUnderlying underlying { get; set; }
        public string strategy { get; set; }
        public int interval { get; set; }
        public bool isDelayed { get; set; }
        public bool isIndex { get; set; }
        public int daysToExpiration { get; set; }
        public int interestRate { get; set; }
        public int underlyingPrice { get; set; }
        public int volatility { get; set; }

        public List<TDOptionMap> callExpDateMap { get; set; }
        public List<TDOptionMap> putExpDateMap { get; set; }
    }

    public class TDOptionChainConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TDOptionChain);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var doc = JObject.Load(reader);
            var model = new TDOptionChain();
            model.symbol = doc["symbol"].Value<string>();
            model.status = doc["status"].Value<string>();
            model.underlying = doc["underlying"].ToObject<TDUnderlying>();
            model.strategy = doc["strategy"].Value<string>();
            model.interval = doc["interval"].Value<int>();
            model.isDelayed = doc["isDelayed"].Value<bool>();
            model.isIndex = doc["isIndex"].Value<bool>();
            model.daysToExpiration = doc["daysToExpiration"].Value<int>();
            model.interestRate = doc["interestRate"].Value<int>();
            model.underlyingPrice = doc["underlyingPrice"].Value<int>();
            model.volatility = doc["volatility"].Value<int>();
            model.callExpDateMap = GetMap(doc["callExpDateMap"].ToObject<JObject>());
            model.putExpDateMap = GetMap(doc["putExpDateMap"].ToObject<JObject>());
            return model;
        }

        public List<TDOptionMap> GetMap(JObject doc)
        {
            var map = new List<TDOptionMap>();

            foreach (var expiry in doc.Properties())
            {
                var exp = new TDOptionMap();
                map.Add(exp);
                exp.expires = DateTime.Parse(expiry.Name.Split(':')[0]);
                exp.options = new List<TDOption>();

                var set = expiry.Value.ToObject<JObject>();
                foreach (var contract in set.Properties())
                {
                    var stike = double.Parse(contract.Name);
                    var tuples = contract.Value.First.ToObject<JObject>();
                    var option = new TDOption();
                    exp.options.Add(option);

                    option.putCall = tuples["putCall"].Value<string>();
                    option.symbol = tuples["symbol"].Value<string>();
                    option.description = tuples["description"].Value<string>();
                    option.exchangeName = tuples["exchangeName"].Value<string>();
                    option.bidPrice = tuples["bid"].Value<double>();
                    option.askPrice = tuples["ask"].Value<double>();
                    option.lastPrice = tuples["last"].Value<double>();
                    option.markPrice = tuples["mark"].Value<double>();
                    option.bidSize = tuples["bidSize"].Value<int>();
                    option.askSize = tuples["askSize"].Value<int>();
                    option.lastSize = tuples["lastSize"].Value<int>();
                    option.highPrice = tuples["highPrice"].Value<double>();
                    option.lowPrice = tuples["lowPrice"].Value<double>();
                    option.openPrice = tuples["openPrice"].Value<double>();
                    option.closePrice = tuples["closePrice"].Value<double>();
                    option.totalVolume = tuples["totalVolume"].Value<int>();
                    option.quoteTimeInLong = tuples["quoteTimeInLong"].Value<long>();
                    option.tradeTimeInLong = tuples["tradeTimeInLong"].Value<long>();
                    option.netChange = tuples["netChange"].Value<double>();
                    option.volatility = tuples["volatility"].Value<double>();
                    option.delta = tuples["delta"].Value<double>();
                    option.gamma = tuples["gamma"].Value<double>();
                    option.theta = tuples["theta"].Value<double>();
                    option.vega = tuples["vega"].Value<double>();
                    option.rho = tuples["rho"].Value<double>();
                    option.timeValue = tuples["timeValue"].Value<double>();
                    option.openInterest = tuples["openInterest"].Value<int>();
                    option.isInTheMoney = tuples["inTheMoney"].Value<bool>();
                    option.theoreticalOptionValue = tuples["theoreticalOptionValue"].Value<double>();
                    option.theoreticalVolatility = tuples["theoreticalVolatility"].Value<double>();
                    option.strikePrice = tuples["strikePrice"].Value<double>();
                    option.expirationDate = tuples["expirationDate"].Value<double>();
                    option.daysToExpiration = tuples["daysToExpiration"].Value<int>();
                    option.multiplier = tuples["multiplier"].Value<double>();
                    option.settlementType = tuples["settlementType"].Value<string>();
                    option.deliverableNote = tuples["deliverableNote"].Value<string>();
                    option.percentChange = tuples["percentChange"].Value<double>();
                    option.markChange = tuples["markChange"].Value<double>();
                    option.markPercentChange = tuples["markPercentChange"].Value<double>();

                }
            }
            return map;
        }

    }


    [Serializable]
    public class TDOptionMap
    {
        public DateTime expires { get; set; }

        public List<TDOption> options { get; set; }
    }



    [Serializable]
    public class TDOption
    {
        public string putCall { get; set; }
        public string symbol { get; set; }
        public string description { get; set; }
        public string exchangeName { get; set; }
        public double bidPrice { get; set; }
        public double askPrice { get; set; }
        public double lastPrice { get; set; }
        public double markPrice { get; set; }
        public int bidSize { get; set; }
        public int askSize { get; set; }
        public int lastSize { get; set; }
        public double highPrice { get; set; }
        public double lowPrice { get; set; }
        public double openPrice { get; set; }
        public double closePrice { get; set; }
        public int totalVolume { get; set; }
        public long quoteTimeInLong { get; set; }
        public long tradeTimeInLong { get; set; }
        public double netChange { get; set; }
        public double volatility { get; set; }
        public double delta { get; set; }
        public double gamma { get; set; }
        public double theta { get; set; }
        public double vega { get; set; }
        public double rho { get; set; }
        public double timeValue { get; set; }
        public int openInterest { get; set; }
        public bool isInTheMoney { get; set; }
        public double theoreticalOptionValue { get; set; }
        public double theoreticalVolatility { get; set; }
        public double strikePrice { get; set; }
        public double expirationDate { get; set; }
        public int daysToExpiration { get; set; }
        public string expirationType { get; set; }
        public double multiplier { get; set; }
        public string settlementType { get; set; }
        public string deliverableNote { get; set; }
        public double percentChange { get; set; }
        public double markChange { get; set; }
        public double markPercentChange { get; set; }
        public DateTime ExpirationDate
        {
            get
            {
                return TDHelpers.FromUnixTimeMilliseconds(expirationDate);
            }
        }
        public DateTime ExpirationDay
        {
            get
            {
                return TDHelpers.ToRegularTradingEnd(DateTime.Now.AddDays(daysToExpiration));
            }
        }
    }

    [Serializable]
    public class TDUnderlying
    {
        public double ask { get; set; }
        public int askSize { get; set; }
        public double bid { get; set; }
        public int bidSize { get; set; }
        public double change { get; set; }
        public double close { get; set; }
        public bool delayed { get; set; }
        public string description { get; set; }
        public string exchangeName { get; set; }
        public double fiftyTwoWeekHigh { get; set; }
        public double fiftyTwoWeekLow { get; set; }
        public double highPrice { get; set; }
        public double last { get; set; }
        public double lowPrice { get; set; }
        public double mark { get; set; }
        public double markChange { get; set; }
        public double markPercentChange { get; set; }
        public double openPrice { get; set; }
        public double percentChange { get; set; }
        public double quoteTime { get; set; }
        public string symbol { get; set; }
        public int totalVolume { get; set; }
        public double tradeTime { get; set; }
    }


    [Serializable]
    public class TDExpirationDate
    {
        public string date { get; set; }
    }

    [Serializable]
    public class TDOptionDeliverables
    {
        public string symbol { get; set; }
        public string assetType { get; set; }
        public string deliverableUnits { get; set; }
        public string currencyType { get; set; }
    }


}

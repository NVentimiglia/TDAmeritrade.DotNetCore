using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace TDAmeritrade
{

    /// <summary>
    /// Utility for deserializing stream messages
    /// </summary>
    public class TDStreamJsonProcessor
    {
        /// <summary> Server Sent Events </summary>
        public event Action<TDHeartbeatSignal> OnHeartbeatSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDChartSignal> OnChartSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDQuoteSignal> OnQuoteSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDTimeSaleSignal> OnTimeSaleSignal = delegate { };
        /// <summary> Server Sent Events </summary>
        public event Action<TDBookSignal> OnBookSignal = delegate { };

        public void Parse(string json)
        {
            var job = JObject.Parse(json);

            if (job.ContainsKey("notify"))
            {
                ParseHeartbeat(job["notify"].First.First.ToObject<long>());
            }
            else if (job.ContainsKey("data"))
            {
                var data = job["data"] as JArray;
                foreach (var item in data)
                {
                    var service = item.Value<string>("service");
                    var contents = item.Value<JArray>("content");
                    var tmstamp = item["timestamp"].Value<long>();

                    if (contents == null)
                        return;

                    foreach (var content in contents.Children<JObject>())
                    {
                        if (service == "QUOTE")
                        {
                            ParseQuote(tmstamp, content);
                        }
                        else if (service == "CHART_FUTURES")
                        {
                            ParseChartFutures(tmstamp, content);
                        }
                        else if (service == "CHART_EQUITY")
                        {
                            ParseChartEquity(tmstamp, content);
                        }
                        else if (service == "LISTED_BOOK" || service == "NASDAQ_BOOK" || service == "OPTIONS_BOOK")
                        {
                            ParseBook(tmstamp, content, service);
                        }
                        else if (service == "TIMESALE_EQUITY" || service == "TIMESALE_FUTURES" || service == "TIMESALE_FOREX" || service == "TIMESALE_OPTIONS")
                        {
                            ParseTimeSaleEquity(tmstamp, content);
                        }
                    }
                }
            }
        }

        void ParseHeartbeat(long tmstamp)
        {
            var model = new TDHeartbeatSignal { timestamp = tmstamp };
            OnHeartbeatSignal(model);
        }

        void ParseBook(long tmstamp, JObject content, string service)
        {
            var model = new TDBookSignal();
            model.timestamp = tmstamp;
            model.id = (TDBookOptions)Enum.Parse(typeof(TDBookOptions), service);
            foreach (var item in content)
            {
                switch (item.Key)
                {
                    case "key":
                        model.symbol = item.Value.Value<string>();
                        break;
                    //case "1":
                    //    model.booktime = item.Value.Value<long>();
                    //    break;
                    case "2":
                        model.bids = (item.Value as JArray).ToObject<TDBookLevel[]>();
                        break;
                    case "3":
                        model.asks = (item.Value as JArray).ToObject<TDBookLevel[]>();
                        break;
                }
            }
            OnBookSignal(model);
        }

        void ParseChartFutures(long tmstamp, JObject content)
        {
            var model = new TDChartSignal();
            model.timestamp = tmstamp;
            foreach (var item in content)
            {
                switch (item.Key)
                {
                    case "key":
                        model.symbol = item.Value.Value<string>();
                        break;
                    case "seq":
                        model.sequence = item.Value.Value<long>();
                        break;
                    case "1":
                        model.charttime = item.Value.Value<long>();
                        break;
                    case "2":
                        model.openprice = item.Value.Value<double>();
                        break;
                    case "3":
                        model.highprice = item.Value.Value<double>();
                        break;
                    case "4":
                        model.lowprice = item.Value.Value<double>();
                        break;
                    case "5":
                        model.closeprice = item.Value.Value<double>();
                        break;
                    case "6":
                        model.volume = item.Value.Value<long>();
                        break;
                }
            }
            OnChartSignal(model);
        }

        void ParseChartEquity(long tmstamp, JObject content)
        {
            var model = new TDChartSignal();
            model.timestamp = tmstamp;
            foreach (var item in content)
            {
                switch (item.Key)
                {
                    case "key":
                        model.symbol = item.Value.Value<string>();
                        break;
                    case "seq":
                        model.sequence = item.Value.Value<long>();
                        break;
                    case "1":
                        model.openprice = item.Value.Value<double>();
                        break;
                    case "2":
                        model.highprice = item.Value.Value<double>();
                        break;
                    case "3":
                        model.lowprice = item.Value.Value<double>();
                        break;
                    case "4":
                        model.closeprice = item.Value.Value<double>();
                        break;
                    case "5":
                        model.volume = item.Value.Value<double>();
                        break;
                    case "6":
                        model.sequence = item.Value.Value<long>();
                        break;
                    case "7":
                        model.charttime = item.Value.Value<long>();
                        break;
                    case "8":
                        model.chartday = item.Value.Value<int>();
                        break;
                }
            }
            OnChartSignal(model);
        }

        void ParseTimeSaleEquity(long tmstamp, JObject content)
        {
            var model = new TDTimeSaleSignal();
            model.timestamp = tmstamp;
            foreach (var item in content)
            {
                switch (item.Key)
                {
                    case "key":
                        model.symbol = item.Value.Value<string>();
                        break;
                    case "seq":
                        model.sequence = item.Value.Value<long>();
                        break;
                    case "1":
                        model.tradetime = item.Value.Value<long>();
                        break;
                    case "2":
                        model.lastprice = item.Value.Value<double>();
                        break;
                    case "3":
                        model.lastsize = item.Value.Value<double>();
                        break;
                    case "4":
                        model.lastsequence = item.Value.Value<long>();
                        break;
                }
            }
            OnTimeSaleSignal(model);
        }

        void ParseQuote(long tmstamp, JObject content)
        {
            var model = new TDQuoteSignal();
            model.timestamp = tmstamp;
            foreach (var item in content)
            {
                switch (item.Key)
                {
                    case "key":
                        model.symbol = item.Value.Value<string>();
                        break;
                    case "1":
                        model.bidprice = item.Value.Value<double>();
                        break;
                    case "2":
                        model.askprice = item.Value.Value<double>();
                        break;
                    case "3":
                        model.lastprice = item.Value.Value<double>();
                        break;
                    case "4":
                        model.bidsize = item.Value.Value<double>();
                        break;
                    case "5":
                        model.asksize = item.Value.Value<double>();
                        break;
                    case "6":
                        model.askid = item.Value.Value<char>();
                        break;
                    case "7":
                        model.bidid = item.Value.Value<char>();
                        break;
                    case "8":
                        model.totalvolume = item.Value.Value<long>();
                        break;
                    case "9":
                        model.lastsize = item.Value.Value<double>();
                        break;
                    case "10":
                        model.tradetime = item.Value.Value<long>();
                        break;
                    case "11":
                        model.quotetime = item.Value.Value<long>();
                        break;
                    case "12":
                        model.highprice = item.Value.Value<double>();
                        break;
                    case "13":
                        model.lowprice = item.Value.Value<double>();
                        break;
                    case "14":
                        model.bidtick = item.Value.Value<char>();
                        break;
                    case "15":
                        model.closeprice = item.Value.Value<double>();
                        break;
                    case "24":
                        model.volatility = item.Value.Value<double>();
                        break;
                    case "28":
                        model.openprice = item.Value.Value<double>();
                        break;
                }
            }
            OnQuoteSignal(model);
        }
    }
}
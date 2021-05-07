using Newtonsoft.Json.Linq;
using System;

namespace TDAmeritrade
{
    /// <summary>
    /// Utility for deserializing stream messages
    /// </summary>
    public class TDAmeritradeJsonParser
    {
        public event Action<long> OnHeartbeat = delegate { };
        public event Action<TDQuoteSignal> OnQuote = delegate { };
        public event Action<TDTimeSaleEquitySignal> OnTimeSaleEquity = delegate { };
        public event Action<TDChartSignal> OnChart = delegate { };

        public void Parse(string json)
        {
            var job = JObject.Parse(json);

            if (job.ContainsKey("notify"))
            {
                OnHeartbeat(job["notify"].First.First.ToObject<long>());
            }
            else if (job.ContainsKey("data"))
            {
                var service = job["data"].First.First.First.Value<string>();
                var content = job["data"].First.Last.First.First as JObject;
                var tmstamp = job["data"].First["timestamp"].Value<long>();

                if (content == null)
                    return;

                if (service == "QUOTE")
                {
                    ParseQuote(tmstamp, content);
                }
                else if (service == "CHART_EQUITY" || service == "CHART_FUTURES" || service == "CHART_OPTIONS")
                {
                    ParseChart(tmstamp, content);
                }
                else if (service == "TIMESALE_EQUITY" || service == "TIMESALE_FUTURES" || service == "TIMESALE_FOREX" || service == "TIMESALE_OPTIONS")
                {
                    ParseTimeSaleEquity(tmstamp, content);
                }
            }
        }

        void ParseChart(long tmstamp, JObject content)
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
            OnChart(model);
        }
        void ParseTimeSaleEquity(long tmstamp, JObject content)
        {
            var model = new TDTimeSaleEquitySignal();
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
            OnTimeSaleEquity(model);
        }

        void ParseQuote(long tmstamp, JObject content)
        {
            var quote = new TDQuoteSignal();
            quote.timestamp = tmstamp;
            foreach (var item in content)
            {
                switch (item.Key)
                {
                    case "key":
                        quote.symbol = item.Value.Value<string>();
                        break;
                    case "1":
                        quote.bidprice = item.Value.Value<double>();
                        break;
                    case "2":
                        quote.askprice = item.Value.Value<double>();
                        break;
                    case "3":
                        quote.lastprice = item.Value.Value<double>();
                        break;
                    case "4":
                        quote.bidsize = item.Value.Value<double>();
                        break;
                    case "5":
                        quote.asksize = item.Value.Value<double>();
                        break;
                    case "6":
                        quote.askid = item.Value.Value<char>();
                        break;
                    case "7":
                        quote.bidid = item.Value.Value<char>();
                        break;
                    case "8":
                        quote.totalvolume = item.Value.Value<long>();
                        break;
                    case "9":
                        quote.lastsize = item.Value.Value<double>();
                        break;
                    case "10":
                        quote.tradetime = item.Value.Value<long>();
                        break;
                    case "11":
                        quote.quotetime = item.Value.Value<long>();
                        break;
                    case "12":
                        quote.highprice = item.Value.Value<double>();
                        break;
                    case "13":
                        quote.lowprice = item.Value.Value<double>();
                        break;
                    case "14":
                        quote.bidtick = item.Value.Value<char>();
                        break;
                    case "15":
                        quote.closeprice = item.Value.Value<double>();
                        break;
                    case "24":
                        quote.volatility = item.Value.Value<double>();
                        break;
                    case "28":
                        quote.openprice = item.Value.Value<double>();
                        break;
                }
            }
            OnQuote(quote);
        }
    }
}
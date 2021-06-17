using Newtonsoft.Json;
using System;

namespace TDAmeritrade
{
    [Serializable]
    public struct TDPriceCandle 
    {
        public double close { get; set; }
        public double datetime { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double volume { get; set; }

        [JsonIgnore]
        public DateTime DateTime
        {
            get
            {
                return TDHelpers.FromUnixTimeSeconds(datetime);
            }
            set
            {
                datetime = TDHelpers.ToUnixTimeSeconds(value);
            }
        }
    }


    [Serializable]
    ///https://developer.tdameritrade.com/content/price-history-samples
    public struct TDPriceHistoryRequest
    {
        [Serializable]
        public enum PeriodTypes
        {
            day,
            month,
            year,
            ytd
        }
        [Serializable]
        public enum FrequencyType
        {
            minute,
            daily,
            weekly,
            monthly
        }

        public string symbol { get; set; }

        /// <summary>
        /// The type of period to show. Valid values are day, month, year, or ytd (year to date). Default is day.
        /// </summary>
        public PeriodTypes? periodType { get; set; }
        /// <summary>
        /// The number of periods to show.
        /// Example: For a 2 day / 1 min chart, the values would be:
        /// period: 2
        /// periodType: day
        /// frequency: 1
        /// frequencyType: min
        /// Valid periods by periodType(defaults marked with an asterisk) :
        /// day: 1, 2, 3, 4, 5, 10*
        /// month: 1*, 2, 3, 6
        /// year: 1*, 2, 3, 5, 10, 15, 20
        /// ytd: 1*
        /// </summary>
        public int period { get; set; }

        /// <summary>
        /// The type of frequency with which a new candle is formed.
        /// Valid frequencyTypes by periodType(defaults marked with an asterisk):
        /// day: minute*
        /// month: daily, weekly*
        /// year: daily, weekly, monthly*
        /// ytd: daily, weekly*
        /// </summary>
        public FrequencyType? frequencyType { get; set; }
        /// <summary>
        /// The number of the frequencyType to be included in each candle.
        /// /// Valid frequencies by frequencyType (defaults marked with an asterisk):   
        /// minute: 1*, 5, 10, 15, 30
        /// daily: 1*
        /// weekly: 1*
        /// monthly: 1*
        /// </summary>
        public int frequency { get; set; }

        /// <summary>
        /// End date as milliseconds since epoch. If startDate and endDate are provided, period should not be provided. Default is previous trading day.
        /// </summary>
        public double? endDate { get; set; }

        /// <summary>
        /// Start date as milliseconds since epoch. If startDate and endDate are provided, period should not be provided.
        /// </summary>
        public double? startDate { get; set; }

        /// <summary>
        /// true to return extended hours data, false for regular market hours only. Default is true
        /// </summary>
        public bool? needExtendedHoursData { get; set; }


        [JsonIgnore]
        public DateTime? EndDate
        {
            get
            {
                return endDate.HasValue ? TDHelpers.FromUnixTimeSeconds(endDate.Value) : (DateTime?)null;
            }
            set
            {
                endDate = value.HasValue ? TDHelpers.ToUnixTimeSeconds(value.Value) : (double?)null;
            }
        }

        [JsonIgnore]
        public DateTime? StartDate
        {
            get
            {
                return startDate.HasValue ? TDHelpers.FromUnixTimeSeconds(startDate.Value) : (DateTime?)null;
            }
            set
            {
                startDate = value.HasValue ? TDHelpers.ToUnixTimeSeconds(value.Value) : (double?)null;
            }
        }
    }

}

using System;
using System.Collections.Generic;

namespace TDAmeritrade
{
    public static class TDHelpers
    {
        public static long UnixSecondsToMiliseconds(this double time)
        {
            return (long)time * 1000;
        }

        public static double UnixMilisecondsToSeconds(long time)
        {
            return time / 1000;
        }

        public static double ToUnixTimeSeconds(this DateTime time)
        {
            TimeSpan t = time - new DateTime(1970, 1, 1);
            return t.TotalSeconds;
        }

        public static DateTime FromUnixTimeSeconds(double time)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(time);
        }

        public static double ToUnixTimeMilliseconds(this DateTime time)
        {
            TimeSpan t = time - new DateTime(1970, 1, 1);
            return t.TotalMilliseconds;
        }
        public static DateTime FromUnixTimeMilliseconds(double time)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(time);
        }

        public static DateTime ToEST(this DateTime time)
        {
            var timeUtc = time.ToUniversalTime();
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
        }

        public static DateTime ToRegularTradingStart(this DateTime time)
        {
            return ToEST(time).Date.AddHours(9).AddMinutes(30);
        }

        public static DateTime ToRegularTradingEnd(this DateTime time)
        {
            var est = ToEST(time);
            return est.Date.AddHours(12 + 4);
        }

        public static DateTime ToPreMarketStart(this DateTime time)
        {
            return ToEST(time).Date.AddHours(7);
        }

        public static DateTime ToPostMarketEnd(this DateTime time)
        {
            var est = ToEST(time);
            return est.Date.AddHours(12 + 8);
        }

        public static bool IsFutureSymbol(string symbol)
        {
            return symbol != null && symbol.Length > 1 && symbol[0] == '/';
        }
        public static bool IsIndexSymbol(string symbol)
        {
            return symbol != null && symbol.Length > 1 && symbol[0] == '$';
        }
        public static bool IsOptionSymbol(string symbol)
        {
            return symbol != null && symbol.Length > 12;
        }
        public static bool IsEquitySymbol(string symbol)
        {
            return symbol != null && !IsFutureSymbol(symbol) && !IsIndexSymbol(symbol) && !IsOptionSymbol(symbol);
        }

        public static int ToCandleIndex(this DateTime time, int minutes)
        {
            var est = time.ToEST();
            var start = time.ToRegularTradingStart();
            var totalMin = est - start;
            var index = (int)totalMin.TotalMinutes / minutes;
            return index;
        }


        /// <summary>
        /// Merges N candles into totalCandles candles.
        /// </summary>
        /// <param name="candles"></param>
        /// <param name="totalCandles">if totalCandles == 10, turn 30 1 minute canldes into (10) 3 minute candles.</param>
        /// <returns></returns>
        public static TDPriceCandle[] ConsolidateByTotalCount(this TDPriceCandle[] candles, int totalCandles)
        {
            var periods = (int)Math.Ceiling((decimal)candles.Length / (decimal)totalCandles);
            return ConsolidateByPeriodCount(candles, periods);
        }

        /// <summary>
        /// Merges N candles X candles divisibable by periodsPerNewCandle candles.
        /// </summary>
        /// <param name="candles"></param>
        /// <param name="periodsToMerge">if periodsPerNewCandle == 3, turn 30 1 minute canldes into 10 (3) minute candles.</param>
        /// <returns></returns>
        public static TDPriceCandle[] ConsolidateByPeriodCount(this TDPriceCandle[] candles, int periodsPerNewCandle)
        {
            var sum = (int)Math.Round((decimal)candles.Length / periodsPerNewCandle, MidpointRounding.AwayFromZero);
            TDPriceCandle[] result = new TDPriceCandle[sum];

            int index = -1;
            for (int i = 0; i < candles.Length; i++)
            {
                if (i % periodsPerNewCandle == 0)
                {
                    index++;
                    result[index] = candles[i];
                }
                else
                {
                    result[index].close = candles[i].close;
                    result[index].volume += candles[i].volume;
                    if (result[index].low > candles[i].low)
                    {
                        result[index].low = candles[i].low;
                    }
                    if (result[index].high < candles[i].high)
                    {
                        result[index].high = candles[i].high;
                    }
                }
            }

            return result;
        }
    }
}

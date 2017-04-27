using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TimeMerge.Utils
{
    public class Calculations
    {
        public static DateTime StringToDateTime(string hourMinute, int year, int month, int day)
        {
            string[] hourMinParts = hourMinute.Split(':');
            int hour = 0;
            int minute = 0;
            if (hourMinParts.Length == 2)
            {
                Int32.TryParse(hourMinParts[0], out hour);
                Int32.TryParse(hourMinParts[1], out minute);
            }
            else
            {
                hourMinute = hourMinute.PadLeft(4, '0');
                Int32.TryParse(hourMinute.Substring(0, 2), out hour);
                Int32.TryParse(hourMinute.Substring(2, 2), out minute);
            }
            return new DateTime(year, month, day, hour, minute, 0);
        }

        public static string DateTimeToHourMinuteString(DateTime dateTime)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:t}", dateTime); // 5 minutes after 7 o'clock => "7:05"
        }

        public static string ParseForAppVersionNumber(string webResponseToParse)
        {
            string versionNumber = null;

            Regex exVersionNumber = new Regex("\\[([0-9.]+)\\]"); // to match e.g. "New TimeMerge version [1.1.5] available"
            var versionNumberMatch = exVersionNumber.Match(webResponseToParse);
            if (versionNumberMatch.Groups.Count > 1 && versionNumberMatch.Groups[1].Captures.Count > 0)
            {
                versionNumber = versionNumberMatch.Groups[1].Captures[0].Value;
            }

            return versionNumber;
        }

        private static DateTime? m_ForcedNowTime;
        public static DateTime NowTime
        {
            get
            {
                return m_ForcedNowTime ?? DateTime.Now;
            }
            set
            {
                m_ForcedNowTime = value;
            }
        }

        public static string MonthBalanceAsHumanString(TimeSpan wholeMonthBalance)
        {
            int absDays = Math.Abs(wholeMonthBalance.Days);
            int absHours = Math.Abs(wholeMonthBalance.Hours);
            int absMins = Math.Abs(wholeMonthBalance.Minutes);

            var ticks = wholeMonthBalance.Ticks;
            return string.Format("{0}{1}:{2:D2}", (ticks < 0) ? "-" : "+", absDays * 24 + absHours, absMins);
        }

        public static TimeSpan ParseHHMMToTimeSpan(string durationToParse)
        {
            string[] timeTokens = durationToParse.Split(':');
            if (timeTokens.Length != 2)
                throw new ArgumentException(string.Format("ParseStringToTimeSpan() cannot parse '{0}'", durationToParse));

            int hours = 0;
            int.TryParse(timeTokens[0], out hours);

            int mins = 0;
            int.TryParse(timeTokens[1], out mins);

            bool isNegativeTimeSpan = (hours < 0);
            if (timeTokens[0].IndexOf('-') != -1) // "-0:02" has zero hours and zero is _not_ negative, so also check for a negative sign
                isNegativeTimeSpan = true;
            if (isNegativeTimeSpan)
                mins = -mins;

            return TimeSpan.FromMinutes(hours * 60 + mins);
        }
    }
}

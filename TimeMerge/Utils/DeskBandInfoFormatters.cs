
using System;
using System.Linq;

namespace TimeMerge.Utils
{
    public interface IDeskBandInfoFormatter
    {
        string GetDeskBandString(TimeMerge.ViewModel.SingleMonthViewModel monthVM);
    }

    public class DeskBandInfoDefaultFormatter : IDeskBandInfoFormatter
    {
        public string GetDeskBandString(ViewModel.SingleMonthViewModel monthVM)
        {
            TimeSpan deskBandInfoBalance = monthVM.BalanceWholeMonth;
            string deskBandInfo = TimeMerge.Utils.Calculations.MonthBalanceAsHumanString(deskBandInfoBalance);
            return deskBandInfo;
        }
    }

    public class DeskBandInfoCurrentDayOnlyFormatter : IDeskBandInfoFormatter
    {
        public string GetDeskBandString(ViewModel.SingleMonthViewModel monthVM)
        {
            TimeSpan dayDuration = new TimeSpan();

            var currentDayVM = (from dayVM in monthVM.Days
                                where dayVM.Day == Calculations.NowTime.Day
                                select dayVM).FirstOrDefault();
            if (currentDayVM != null)
                dayDuration = currentDayVM.Duration;

            string deskBandString = TimeMerge.Utils.Calculations.MonthBalanceAsHumanString(dayDuration);
            deskBandString = deskBandString.Replace("+", "");

            if (   Calculations.NowTime.Year  != monthVM.YearMonth.Year
                || Calculations.NowTime.Month != monthVM.YearMonth.Month)
            {
                deskBandString = "???";
            }

            return deskBandString;
        }
    }
}
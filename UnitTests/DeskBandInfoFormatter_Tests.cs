
using TimeMerge.ViewModel;
using TimeMerge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    class FakeMainViewModel : MainViewModel
    {
        protected override HomeOfficeDetector createHomeOfficeDetector()
        {
            return null;
        }
    }

    class FakeMonthViewModel : SingleMonthViewModel
    {
        public FakeMonthViewModel()
            : base(new FakeMainViewModel(), new NoActionOnWebRequest(), new WebDavConnection())
        { }

        public TimeSpan ForcedBalanceWholeMonth { get; set; }
        public override TimeSpan BalanceWholeMonth
        {
            get { return ForcedBalanceWholeMonth; }
        }

        public DateTime ForcedYearMonth { get; set; }
        public override DateTime YearMonth
        {
            get { return ForcedYearMonth; }
        }

        public void AddDayVM(SingleDayViewModel dayVM)
        {
            this._days.Add(dayVM);
        }
    }

    class FakeDayViewModel : SingleDayViewModel
    {
        public TimeSpan ForcedDuration { get; set; }
        public override TimeSpan Duration
        {
            get { return ForcedDuration; }
        }

        public int ForcedDay { get; set; }
        public override int Day
        {
            get { return ForcedDay; }
        }
    }

    [TestClass]
    public class DeskBandInfoDefaultFormatter_Tests
    {
        [TestMethod]
        public void GetDeskBandInfo_ReturnsBalanceWholeMonth_Always()
        {
            TimeSpan monthBalance = TimeSpan.FromMinutes(3 * 60 + 10);
            FakeMonthViewModel monthVM = new FakeMonthViewModel();
            monthVM.ForcedBalanceWholeMonth = monthBalance;

            var formatter = new DeskBandInfoDefaultFormatter();
            string deskBandString = formatter.GetDeskBandString(monthVM);

            Assert.AreEqual("+3:10", deskBandString);
        }
    }

    [TestClass]
    public class DeskBandInfoCurrentDayOnlyFormatter_Tests
    {
        [TestMethod]
        public void GetDeskBandInfo_ReturnsZeroHoursZeroMinutes_WhenCurrentDayDurationEmpty()
        {
            Calculations.NowTime = new DateTime(2015, 4, 1);
            TimeSpan monthBalance = TimeSpan.FromMinutes(3 * 60 + 10);
            FakeMonthViewModel monthVM = new FakeMonthViewModel() { ForcedYearMonth = Calculations.NowTime };
            monthVM.ForcedBalanceWholeMonth = monthBalance;

            var formatter = new DeskBandInfoCurrentDayOnlyFormatter();
            string deskBandString = formatter.GetDeskBandString(monthVM);

            Assert.AreEqual("0:00", deskBandString); // instead of month balance of "+3:10"
        }

        [TestMethod]
        public void GetDeskBandInfo_ReturnsCurrentDayDuration_WhenCurrentDayDurationNotEmpty()
        {
            Calculations.NowTime = new DateTime(2015, 5, 1);
            TimeSpan monthBalance = TimeSpan.FromMinutes(3 * 60 + 10);
            FakeMonthViewModel monthVM = new FakeMonthViewModel() { ForcedYearMonth = Calculations.NowTime };
            monthVM.ForcedBalanceWholeMonth = monthBalance;
            FakeDayViewModel dayVm1 = new FakeDayViewModel() { ForcedDay = 1, ForcedDuration = TimeSpan.FromHours(6.5) };
            monthVM.AddDayVM(dayVm1);

            var formatter = new DeskBandInfoCurrentDayOnlyFormatter();
            string deskBandString = formatter.GetDeskBandString(monthVM);

            Assert.AreEqual("6:30", deskBandString); // instead of month balance of "+3:10"
        }

        [TestMethod]
        public void GetDeskBandInfo_ReturnsUnknownDuration_WhenCalledForMonthInThePast()
        {
            TimeSpan monthBalance = TimeSpan.FromMinutes(3 * 60 + 10);
            FakeMonthViewModel monthVM = new FakeMonthViewModel() { ForcedYearMonth = new DateTime(2015, 2, 1) };
            monthVM.ForcedBalanceWholeMonth = monthBalance;
            Calculations.NowTime = new DateTime(2015, 5, 3);
            FakeDayViewModel dayVm1 = new FakeDayViewModel() { ForcedDay = 1, ForcedDuration = TimeSpan.FromHours(6.5) };
            FakeDayViewModel dayVm2 = new FakeDayViewModel() { ForcedDay = 2, ForcedDuration = TimeSpan.FromHours(7.0) };
            FakeDayViewModel dayVm3 = new FakeDayViewModel() { ForcedDay = 3, ForcedDuration = TimeSpan.FromHours(7.5) };
            monthVM.AddDayVM(dayVm1);
            monthVM.AddDayVM(dayVm2);
            monthVM.AddDayVM(dayVm3);

            var formatter = new DeskBandInfoCurrentDayOnlyFormatter();
            string deskBandString = formatter.GetDeskBandString(monthVM);

            Assert.AreEqual("???", deskBandString); // can only be calculated for the current month, but not for months in the past
        }
    }
}
using TimeMerge.Model;
using TimeMerge.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for SingleDayDataTest and is intended
    ///to contain all SingleDayDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SingleDayDataTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void SplitWorkSpanTest()
        {
            // We shall turn intervals like this...
            //  7:00 - 15:00	17:17 - 18:18
            //
            // ...into intervals like this:
            //  7:00 - 12:00	12:20 - 15:00	17:17 - 18:18

            SingleDayData dayData = new SingleDayData();
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 2, 7,0,0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 2, 15,0,0);
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2, 17, 17, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 2, 18, 18, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkSpanWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsTrue(inserted);
            
            Assert.AreEqual(12, dayData.WorkSpans[0].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkSpans[0].GetEndTimeIncludingCorrections().Minute);
            Assert.AreEqual(12, dayData.WorkSpans[1].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(20, dayData.WorkSpans[1].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(15, dayData.WorkSpans[1].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkSpans[1].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.PDOMA, dayData.WorkInterruptions[1].GetTypeIncludingCorrections());
            Assert.AreEqual(17, dayData.WorkInterruptions[1].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(17, dayData.WorkInterruptions[1].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(18, dayData.WorkInterruptions[1].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(18, dayData.WorkInterruptions[1].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.OBED, dayData.WorkInterruptions[0].GetTypeIncludingCorrections());
            Assert.AreEqual(12, dayData.WorkInterruptions[0].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[0].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(12, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(20, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Minute);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillAcceptInterruption_WhenLunchTimeFitsNicelyInsideWorkTime()
        {
            // We shall turn interruptions like this...
            //  7:00 - 15:00 (PDOMA)	17:17 - 18:18 (SLUZ)
            //
            // ...into intervals like this:
            //  7:00 - 12:00 (PDOMA)	12:00 - 12:20 (OBED)    12:20 - 15:00 (PDOMA)	17:17 - 18:18 (SLUZ)

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2, 7, 0, 0);
            dayData.WorkInterruptions[0].EndTime   = new DateTime(2012, 10, 2, 15, 0, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;
            dayData.WorkInterruptions[1].StartTime = new DateTime(2012, 10, 2, 17, 17, 0);
            dayData.WorkInterruptions[1].EndTime   = new DateTime(2012, 10, 2, 18, 18, 0);
            dayData.WorkInterruptions[1].Type = WorkInterruption.WorkInterruptionType.SLUZ;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsTrue(inserted);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.PDOMA, dayData.WorkInterruptions[0].GetTypeIncludingCorrections());
            Assert.AreEqual(7, dayData.WorkInterruptions[0].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[0].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(12, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.OBED, dayData.WorkInterruptions[1].GetTypeIncludingCorrections());
            Assert.AreEqual(12, dayData.WorkInterruptions[1].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[1].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(12, dayData.WorkInterruptions[1].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(20, dayData.WorkInterruptions[1].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.PDOMA, dayData.WorkInterruptions[2].GetTypeIncludingCorrections());
            Assert.AreEqual(12, dayData.WorkInterruptions[2].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(20, dayData.WorkInterruptions[2].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(15, dayData.WorkInterruptions[2].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[2].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.SLUZ, dayData.WorkInterruptions[3].GetTypeIncludingCorrections());
            Assert.AreEqual(17, dayData.WorkInterruptions[3].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(17, dayData.WorkInterruptions[3].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(18, dayData.WorkInterruptions[3].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(18, dayData.WorkInterruptions[3].GetEndTimeIncludingCorrections().Minute);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillRejectInterruption_WhenWholeWorkTimeBeforeLunchTime()
        {
            // We shall turn interruptions like this...
            //  7:00 - 11:40 (PDOMA)
            //
            // ...into this:
            //  7:00 - 11:40 (PDOMA)

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2,  7,  0, 0);
            dayData.WorkInterruptions[0].EndTime   = new DateTime(2012, 10, 2, 11, 40, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsFalse(inserted);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillRejectInterruption_WhenWholeWorkTimeAfterLunchTime()
        {
            // We shall turn interruptions like this...
            //  13:00 - 19:40 (PDOMA)
            //
            // ...into this:
            //  13:00 - 19:40 (PDOMA)

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2, 13,  0, 0);
            dayData.WorkInterruptions[0].EndTime   = new DateTime(2012, 10, 2, 19, 40, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsFalse(inserted);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillRejectInterruption_WhenWholeWorkEndsDuringLunchTime()
        {
            // We shall turn interruptions like this...
            //  7:00 - 12:10 (PDOMA)
            //
            // ...into this:
            //  7:00 - 12:10 (PDOMA)

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2,  7,  0, 0);
            dayData.WorkInterruptions[0].EndTime   = new DateTime(2012, 10, 2, 12, 10, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsFalse(inserted);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillRejectInterruption_WhenWholeWorkStartsDuringLunchTime()
        {
            // We shall turn interruptions like this...
            // 12:10 - 19:00 (PDOMA)
            //
            // ...into this:
            // 12:10 - 19:00 (PDOMA)

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2, 12, 10, 0);
            dayData.WorkInterruptions[0].EndTime   = new DateTime(2012, 10, 2, 19,  1, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsFalse(inserted);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillAcceptInterruption_WhenWorkStartedBeforeLunchTimeButWorkStillNotFinished()
        {
            // We shall turn interruptions like this...
            //  7:00 - xxx (PDOMA)
            //
            // ...into this:
            //  7:00 - 12:00 (PDOMA)	12:00 - 12:20 (OBED)    12:20 - xxx (PDOMA)

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 2,  7,  0, 0);
            // dayData.WorkInterruptions[0].EndTime   = ...;
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsTrue(inserted);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.PDOMA, dayData.WorkInterruptions[0].GetTypeIncludingCorrections());
            Assert.AreEqual(7, dayData.WorkInterruptions[0].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[0].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(2012, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Year); // indefinite value is "0:00" in year "1", so date shall be set anew, as well
            Assert.AreEqual(10, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Month);
            Assert.AreEqual(2, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Day);
            Assert.AreEqual(12, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[0].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.OBED, dayData.WorkInterruptions[1].GetTypeIncludingCorrections());
            Assert.AreEqual(12, dayData.WorkInterruptions[1].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[1].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(12, dayData.WorkInterruptions[1].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(20, dayData.WorkInterruptions[1].GetEndTimeIncludingCorrections().Minute);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.PDOMA, dayData.WorkInterruptions[2].GetTypeIncludingCorrections());
            Assert.AreEqual(12, dayData.WorkInterruptions[2].GetStartTimeIncludingCorrections().Hour);
            Assert.AreEqual(20, dayData.WorkInterruptions[2].GetStartTimeIncludingCorrections().Minute);
            Assert.AreEqual(0, dayData.WorkInterruptions[2].GetEndTimeIncludingCorrections().Hour);
            Assert.AreEqual(0, dayData.WorkInterruptions[2].GetEndTimeIncludingCorrections().Minute);
        }

        [TestMethod()]
        public void SplitWorkInterruption_WillRejectInterruption_WhenWorkNotEvenStartedAtAll()
        {
            // We shall turn interruptions like this...
            //  <whole day fully empty>
            //
            // ...into this:
            //  <whole day fully empty>

            SingleDayData dayData = new SingleDayData();
            // dayData.WorkInterruptions[0].StartTime = ...;
            // dayData.WorkInterruptions[0].EndTime   = ...;
            // dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            bool inserted = dayData.SplitWorkInterruptionWithInterruption(0, TimeSpan.FromHours(12), TimeSpan.FromMinutes(12 * 60 + SingleDayData.LunchTimeMinimalTime), WorkInterruption.WorkInterruptionType.OBED);
            Assert.IsFalse(inserted);
        }

        [TestMethod()]
        public void WorkSpan0ToolTip_UsesInvariantCulture_Always()
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("sk");
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 11, 11, 59);
            var mainVM = new FakeMainViewModel();
            mainVM.Init();
            var monthData = new TimeMerge.Model.SingleMonthData() { YearMonth = new DateTime(2012, 10, 1) };
            mainVM.MonthViewModel.SetMonthData(monthData);
            TimeMerge.Model.SingleDayData dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };

            //               now |11:11|
            // 6:36 - 
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkInterruptions[0].CorrectionStartTime = new DateTime(2012, 10, 26, 6, 36, 0);
            //             dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[0].CorrectedType = WorkInterruption.WorkInterruptionType.PDOMA;
            TimeMerge.ViewModel.SingleDayViewModel dayVM = new TimeMerge.ViewModel.SingleDayViewModel();
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);
            mainVM.MonthViewModel.Days.Add(dayVM);

            string tooltip = dayVM.Interrupt0ToolTip;

            Assert.AreEqual("Korekcia z 00:00 - 00:00, z typu PDOMA" + System.Environment.NewLine + "+0:00 sa dosiahne o 14:36 (s obedom o 14:56)", tooltip, false);
            Thread.CurrentThread.CurrentCulture = oldCulture;
        }

        [TestMethod()]
        public void TimeToReachZeroBalanceTest()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 15, 48, 59);

            var mainVM = new FakeMainViewModel();
            mainVM.Init();

            var monthData = new TimeMerge.Model.SingleMonthData() { YearMonth = new DateTime(2012, 10, 1) };
            mainVM.MonthViewModel.SetMonthData(monthData);

            TimeMerge.Model.SingleDayData dayData;
            TimeMerge.ViewModel.SingleDayViewModel dayVM = new TimeMerge.ViewModel.SingleDayViewModel();

            //                     now |15:48|
            // 7:00 - 14:00
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 14, 0, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.IsNull(dayVM.WorkSpan0ToolTip);

            //                     now |15:48|
            // 7:00 - .
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 0, 0, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.AreEqual("+0:00 bol dosiahnutý o 15:00 (s obedom o 15:20)", dayVM.WorkSpan0ToolTip, false);

            //                     now |15:48|
            // 10:00 - .
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 10, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 0, 0, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.AreEqual("+0:00 sa dosiahne o 18:00 (s obedom o 18:20)", dayVM.WorkSpan0ToolTip, false);

            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 11, 11, 59);

            //               now |11:11|
            // 7:00 - 9:00                 12:12 - .
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 9, 0, 0);
            dayData.WorkSpans[1].StartTime = new DateTime(2012, 10, 26, 12, 12, 0);
            dayData.WorkSpans[1].EndTime = new DateTime(2012, 10, 26, 0, 0, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.IsNull(dayVM.WorkSpan0ToolTip);
            Assert.AreEqual("+0:00 sa dosiahne o 18:12 (s obedom o 18:32)", dayVM.WorkSpan1ToolTip, false);

            //               now |11:11|
            // 7:00 - 9:00                 12:12 - 18:18
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 9, 0, 0);
            dayData.WorkSpans[1].StartTime = new DateTime(2012, 10, 26, 12, 12, 0);
            dayData.WorkSpans[1].EndTime = new DateTime(2012, 10, 26, 18, 18, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);
            mainVM.MonthViewModel.NotifyPropertyChanged("BalanceWholeMonth");

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.IsNull(dayVM.WorkSpan0ToolTip);
            Assert.IsNull(dayVM.WorkSpan1ToolTip);
            Assert.AreEqual(TimeSpan.FromMinutes(6), mainVM.MonthViewModel.BalanceWholeMonth);




            // Work Spans:                                  Work Interruptions:
            //                                              06:36 - .
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 8, 30, 59);

            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkInterruptions[0].CorrectionStartTime = new DateTime(2012, 10, 26, 6, 36, 0);
            //             dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[0].CorrectedType = WorkInterruption.WorkInterruptionType.PDOMA;
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.AreEqual("Korekcia z 00:00 - 00:00, z typu PDOMA" + System.Environment.NewLine + "+0:00 sa dosiahne o 14:36 (s obedom o 14:56)", dayVM.Interrupt0ToolTip, false);







            // Work Spans:                                  Work Interruptions:
            //                                              7:10 - 11:50 [PDOMA]   11:50 - 12:00 [ZP]   12:00 - 12:20 [OBED]   12:20 - . [PDOMA]
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 13, 40, 59);

            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkInterruptions[0].CorrectionStartTime = new DateTime(2012, 10, 26, 7, 10, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 11, 50, 0);
            dayData.WorkInterruptions[0].CorrectedType = WorkInterruption.WorkInterruptionType.PDOMA;
            dayData.WorkInterruptions[1].CorrectionStartTime = new DateTime(2012, 10, 26, 11, 50, 0);
            dayData.WorkInterruptions[1].EndTime = new DateTime(2012, 10, 26, 12, 0, 0);
            dayData.WorkInterruptions[1].CorrectedType = WorkInterruption.WorkInterruptionType.ZP;
            dayData.WorkInterruptions[2].CorrectionStartTime = new DateTime(2012, 10, 26, 12, 0, 0);
            dayData.WorkInterruptions[2].EndTime = new DateTime(2012, 10, 26, 12, 20, 0);
            dayData.WorkInterruptions[2].CorrectedType = WorkInterruption.WorkInterruptionType.OBED;
            dayData.WorkInterruptions[3].CorrectionStartTime = new DateTime(2012, 10, 26, 12, 20, 0);
            // dayData.WorkInterruptions[3].EndTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[3].CorrectedType = WorkInterruption.WorkInterruptionType.PDOMA;
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            mainVM.MonthViewModel.Days.Clear();
            mainVM.MonthViewModel.Days.Add(dayVM);
            Assert.AreEqual("Korekcia z 00:00 - 00:00, z typu PDOMA" + System.Environment.NewLine + "+0:00 sa dosiahne o 15:30", dayVM.Interrupt3ToolTip, false);
        }

        [TestMethod()]
        public void GetLunchDurationTest()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 15, 48, 59);

            var mainVM = new FakeMainViewModel();
            mainVM.Init();

            var monthData = new TimeMerge.Model.SingleMonthData() { YearMonth = new DateTime(2012, 10, 1) };
            mainVM.MonthViewModel.SetMonthData(monthData);

            TimeMerge.Model.SingleDayData dayData;
            TimeMerge.ViewModel.SingleDayViewModel dayVM = new TimeMerge.ViewModel.SingleDayViewModel();


            
            // Work Spans:
            // 7:00 - 14:00
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 14, 0, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            Assert.AreEqual(TimeSpan.FromMinutes(0), dayVM.GetLunchDuration());


            // Work Spans:                                  Work Interruptions:
            // 7:00 - 12:00   12:20 - 15:00                 12:00 - 12:10 [OBED]   12:10 - 12:20 [OBED]
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 12, 0, 0);
            dayData.WorkSpans[1].StartTime = new DateTime(2012, 10, 26, 12, 20, 0);
            dayData.WorkSpans[1].EndTime = new DateTime(2012, 10, 26, 15, 0, 0);
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 26, 12, 0, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.OBED;
            dayData.WorkInterruptions[1].StartTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[1].EndTime = new DateTime(2012, 10, 26, 12, 20, 0);
            dayData.WorkInterruptions[1].Type = WorkInterruption.WorkInterruptionType.OBED;
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            Assert.AreEqual(TimeSpan.FromMinutes(SingleDayData.LunchTimeMinimalTime), dayVM.GetLunchDuration());



            // Work Spans:                                  Work Interruptions:
            //                                              6:36 - . []
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 26, 8, 30, 59);
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkInterruptions[0].CorrectionStartTime = new DateTime(2012, 10, 26, 6, 36, 0);
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            Assert.AreEqual(TimeSpan.FromMinutes(0), dayVM.GetLunchDuration());



            // Work Spans:
            // 7:00 - 9:00   10:30 - 11:00   11:01 - 11:10   11:11 - 11:20   11:31 - 11:40   11:41 - 12:00   12:20 - 15:00
            // Work Interruptions:
            // 9:00 - 10:30 [LEK]   11:00 - 11:01 [ZP]   11:10 - 11:11 [ZP]   11:20 - 11:21 [ZP]   11:21 - 11:31 [ZP]   11:40 - 11:41 [ZP]   12:00 - 12:10 [OBED]   12:10 - 12:20 [OBED]
            dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 9, 0, 0);
            dayData.WorkSpans[1].StartTime = new DateTime(2012, 10, 26, 10, 30, 0);
            dayData.WorkSpans[1].EndTime = new DateTime(2012, 10, 26, 11, 0, 0);
            dayData.WorkSpans[2].StartTime = new DateTime(2012, 10, 26, 11, 1, 0);
            dayData.WorkSpans[2].EndTime = new DateTime(2012, 10, 26, 11, 10, 0);
            dayData.WorkSpans[3].StartTime = new DateTime(2012, 10, 26, 11, 11, 0);
            dayData.WorkSpans[3].EndTime = new DateTime(2012, 10, 26, 11, 20, 0);
            dayData.WorkSpans[4].StartTime = new DateTime(2012, 10, 26, 11, 31, 0);
            dayData.WorkSpans[4].EndTime = new DateTime(2012, 10, 26, 11, 40, 0);
            dayData.WorkSpans[5].StartTime = new DateTime(2012, 10, 26, 11, 41, 0);
            dayData.WorkSpans[5].EndTime = new DateTime(2012, 10, 26, 12, 0, 0);
            dayData.WorkSpans[6].StartTime = new DateTime(2012, 10, 26, 12, 20, 0);
            dayData.WorkSpans[6].EndTime = new DateTime(2012, 10, 26, 15, 0, 0);
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 26, 9, 0, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 10, 30, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.LEK;
            dayData.WorkInterruptions[1].StartTime = new DateTime(2012, 10, 26, 11, 0, 0);
            dayData.WorkInterruptions[1].EndTime = new DateTime(2012, 10, 26, 11, 1, 0);
            dayData.WorkInterruptions[1].Type = WorkInterruption.WorkInterruptionType.ZP;
            dayData.WorkInterruptions[2].StartTime = new DateTime(2012, 10, 26, 11, 10, 0);
            dayData.WorkInterruptions[2].EndTime = new DateTime(2012, 10, 26, 11, 11, 0);
            dayData.WorkInterruptions[2].Type = WorkInterruption.WorkInterruptionType.ZP;
            dayData.WorkInterruptions[3].StartTime = new DateTime(2012, 10, 26, 11, 20, 0);
            dayData.WorkInterruptions[3].EndTime = new DateTime(2012, 10, 26, 11, 21, 0);
            dayData.WorkInterruptions[3].Type = WorkInterruption.WorkInterruptionType.ZP;
            dayData.WorkInterruptions[4].StartTime = new DateTime(2012, 10, 26, 11, 21, 0);
            dayData.WorkInterruptions[4].EndTime = new DateTime(2012, 10, 26, 11, 31, 0);
            dayData.WorkInterruptions[4].Type = WorkInterruption.WorkInterruptionType.ZP;
            dayData.WorkInterruptions[5].StartTime = new DateTime(2012, 10, 26, 11, 40, 0);
            dayData.WorkInterruptions[5].EndTime = new DateTime(2012, 10, 26, 11, 41, 0);
            dayData.WorkInterruptions[5].Type = WorkInterruption.WorkInterruptionType.ZP;
            dayData.WorkInterruptions[6].StartTime = new DateTime(2012, 10, 26, 12, 0, 0);
            dayData.WorkInterruptions[6].EndTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[6].Type = WorkInterruption.WorkInterruptionType.OBED;
            dayData.WorkInterruptions[7].StartTime = new DateTime(2012, 10, 26, 12, 10, 0);
            dayData.WorkInterruptions[7].EndTime = new DateTime(2012, 10, 26, 12, 20, 0);
            dayData.WorkInterruptions[7].Type = WorkInterruption.WorkInterruptionType.OBED;
            dayVM.ReInit(dayData, mainVM.MonthViewModel, monthData.Days);

            Assert.AreEqual(TimeSpan.FromMinutes(SingleDayData.LunchTimeMinimalTime), dayVM.GetLunchDuration());
        }

        [TestMethod()]
        public void GetDate_WillReturnWorkSpanDate_ByDefault()
        {
            SingleDayData dayData = new SingleDayData();
            dayData.WorkSpans[0].StartTime = new DateTime(2014, 12, 11, 7, 0, 0);
            dayData.WorkSpans[0].EndTime   = new DateTime(2014, 12, 11, 15, 0, 0);

            DateTime singleDayDataDate = dayData.GetDate();

            Assert.AreEqual(new DateTime(2014, 12, 11), singleDayDataDate);
        }

        [TestMethod()]
        public void GetDate_WillReturnWorkInterruptionDate_WhenNoWorkSpanExists()
        {
            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].StartTime = new DateTime(2014, 12, 11, 7, 0, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2014, 12, 11, 15, 0, 0);
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PDOMA;

            DateTime singleDayDataDate = dayData.GetDate();

            Assert.AreEqual(new DateTime(2014, 12, 11), singleDayDataDate);
        }

        [TestMethod()]
        public void Duration_WillAutoDecrementLunchTime_ForPastDays()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2014, 12, 12, 15, 0, 0);

            SingleDayData dayData = new SingleDayData();
            dayData.WorkSpans[0].StartTime = new DateTime(2014, 12, 11, 7, 0, 0);
            dayData.WorkSpans[0].EndTime   = new DateTime(2014, 12, 11, 15, 0, 0);

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60 - SingleDayData.LunchTimeMinimalTime), dayData.Duration);
        }

        [TestMethod()]
        public void Duration_WillNotDecrementLunchTime_ForCurrentDay()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2014, 12, 12, 15, 0, 0);

            SingleDayData dayData = new SingleDayData();
            dayData.WorkSpans[0].StartTime = new DateTime(2014, 12, 12, 7, 0, 0);
            // no EndTime exists yet: still in work

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60), dayData.Duration);
        }

        [TestMethod()]
        public void Duration_WillNotDecrementLunchTime_ForEmptyDays()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2014, 12, 12, 15, 0, 0);

            SingleDayData dayData = new SingleDayData(); // day completely empty, like a weekend-day

            Assert.AreEqual(TimeSpan.FromMinutes(0), dayData.Duration);
        }

        [TestMethod()]
        public void Duration_WillNotDecrementLunchTime_WhenMoreThanOneWorkSpanExists()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2014, 12, 12, 15, 0, 0);

            SingleDayData dayData = new SingleDayData();
            dayData.WorkSpans[0].StartTime = new DateTime(2014, 12, 11,  7,  0, 0);
            dayData.WorkSpans[0].EndTime   = new DateTime(2014, 12, 11, 12,  0, 0);
            dayData.WorkSpans[1].StartTime = new DateTime(2014, 12, 11, 12, 20, 0);
            dayData.WorkSpans[1].EndTime   = new DateTime(2014, 12, 11, 15, 20, 0);

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60), dayData.Duration);
        }

        [TestMethod()]
        public void Duration_WillAutoDecrementLunchTime_ForWorkDayFromHome()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2014, 12, 12, 15, 0, 0);

            SingleDayData dayData = new SingleDayData();
            dayData.WorkInterruptions[0].CorrectionStartTime = new DateTime(2014, 12, 11, 7, 0, 0);
            dayData.WorkInterruptions[0].CorrectionEndTime   = new DateTime(2014, 12, 11, 15, 0, 0);
            dayData.WorkInterruptions[0].CorrectedType = WorkInterruption.WorkInterruptionType.PDOMA;

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60 - SingleDayData.LunchTimeMinimalTime), dayData.Duration);
        }

        [TestMethod()]
        public void DayDuration_WontAutoSubtractLunchTime_WhenWorkdayTooShort()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 27, 15, 48, 59);

            // Work Spans:
            // 7:00 - 13:25 (6 hours 25 minutes) the maximum workday duration that does not yet require a lunch break
            var dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 13, 25, 0);

            Assert.AreEqual(TimeSpan.FromMinutes(6 * 60 + 25), dayData.Duration);
        }

        [TestMethod()]
        public void DayDuration_WontAutoSubtractLunchTime_WhenDayIsBusinessTrip()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 27, 15, 48, 59);

            // Work Interruptions:
            // 7:00 - 15:00 (8 hours) SLUZ
            var dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 15, 0, 0);
            dayData.WorkInterruptions[0].Type = TimeMerge.Model.WorkInterruption.WorkInterruptionType.SLUZ;

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60), dayData.Duration);
        }

        [TestMethod()]
        public void DayDuration_WontAutoSubtractLunchTime_WhenDayContainsSomeBusinessTrip()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 27, 15, 48, 59);

            // Work Spans:
            // 7:00 - 7:10 (0 hours 10 minutes)
            // Work Interruptions:
            // 7:10 - 15:00 (7 hours 50 minutes) SLUZ
            var dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 7, 10, 0);
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 26, 7, 10, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 15, 0, 0);
            dayData.WorkInterruptions[0].Type = TimeMerge.Model.WorkInterruption.WorkInterruptionType.SLUZ;

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60), dayData.Duration);
        }

        [TestMethod()]
        public void DayDuration_WontAutoSubtractLunchTime_WhenDayContainsSomeVacation()
        {
            TimeMerge.Utils.Calculations.NowTime = new DateTime(2012, 10, 27, 15, 48, 59);

            // Work Spans:
            // 7:00 - 11:00 (4 hours)
            // Work Interruptions:
            // 11:00 - 15:00 (4 hours) DOV
            var dayData = new TimeMerge.Model.SingleDayData() { Day = 26, IsNoWorkDay = false };
            dayData.WorkSpans[0].StartTime = new DateTime(2012, 10, 26, 7, 0, 0);
            dayData.WorkSpans[0].EndTime = new DateTime(2012, 10, 26, 11, 0, 0);
            dayData.WorkInterruptions[0].StartTime = new DateTime(2012, 10, 26, 11, 0, 0);
            dayData.WorkInterruptions[0].EndTime = new DateTime(2012, 10, 26, 15, 0, 0);
            dayData.WorkInterruptions[0].Type = TimeMerge.Model.WorkInterruption.WorkInterruptionType.DOV;

            Assert.AreEqual(TimeSpan.FromMinutes(8 * 60), dayData.Duration);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsTrue_OnWorkdaysForPN()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = false }; // simulate a normal, working weekday
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PN;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(true, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsFalse_OnWeekendsAndHolidaysForPN()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = true }; // simulate a weekend/holiday/vacation day
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.PN;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(false, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsTrue_OnWorkdaysForLEK()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = false }; // simulate a normal, working weekday
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.LEK;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(true, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsFalse_OnWeekendsAndHolidaysForLEK()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = true }; // simulate a weekend/holiday/vacation day
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.LEK;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(false, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsTrue_OnWorkdaysForOCR()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = false }; // simulate a normal, working weekday
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.OCR;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(true, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsFalse_OnWeekendsAndHolidaysForOCR()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = true }; // simulate a weekend/holiday/vacation day
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.OCR;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(false, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsTrue_OnWorkdaysForDOV()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = false }; // simulate a normal, working weekday
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.DOV;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(true, isWorkAsWell);
        }

        [TestMethod()]
        public void IsWorkAsWell_ReturnsFalse_OnWeekendsAndHolidaysForDOV()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = true }; // simulate a weekend/holiday/vacation day
            dayData.WorkInterruptions[0].Type = WorkInterruption.WorkInterruptionType.DOV;

            bool isWorkAsWell = dayData.WorkInterruptions[0].IsWorkAsWell;

            Assert.AreEqual(false, isWorkAsWell);
        }

        [TestMethod()]
        public void HomeOfficeDay_WillBeStarted_AccordingToTimeInArguments()
        {
            var dayData = new SingleDayData() { Day = 26, IsNoWorkDay = false }; // simulate a normal, working weekday
            Calculations.NowTime = new DateTime(2016, 10, 31, 7, 5, 0); // it's 07:05 in the morning

            // Remote Desktop Logon happened at, say, 06:38, when TimeMerge was not running yet:
            var remoteLogonTime = new DateTime(2016, 10, 31, 6, 38, 0);
            var args = new HomeOfficeDetector.HomeOfficeActivityEventArgs() { EventTime = remoteLogonTime };
            dayData.StartHomeOfficeDay(args);

            Assert.AreEqual(WorkInterruption.WorkInterruptionType.PDOMA, dayData.WorkInterruptions[0].CorrectedType);
            Assert.AreEqual(remoteLogonTime, dayData.WorkInterruptions[0].CorrectionStartTime);
        }
    }
}

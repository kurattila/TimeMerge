using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.Utils;

namespace UnitTests
{
    class Fake_HomeOfficeDetector : HomeOfficeDetector
    {
        public uint ForcedEnvironmentTickCount { get; set; }
        public bool ForcedIsRemoteSession { get; set; }
        public bool ForcedIsDesktopLocked = false;
        public uint ForcedMostRecentLogonTime { get; set; }
        public void ForceFireHomeOfficeActivityOfDayDetected(uint tickCount)
        {
            fireHomeOfficeActivityOfDayDetected(tickCount);
        }
        public uint ForcedLastInputTime { get; set; }
        protected override uint getLastInputTime()
        {
            return ForcedLastInputTime;
        }
        protected override uint getMostRecentRemoteLogonTime()
        {
            return ForcedMostRecentLogonTime;
        }
        protected override uint getEnvironmentTickCount()
        {
            return ForcedEnvironmentTickCount;
        }
        protected override bool isRemoteSessionActive()
        {
            return ForcedIsRemoteSession;
        }
        protected override bool isDesktopLocked()
        {
            return ForcedIsDesktopLocked;
        }

        public uint ForcedPublic_tickCountFromDateTime(DateTime dateTime)
        {
            return tickCountFromDateTime(dateTime);
        }
    }

    [TestClass]
    public class HomeOfficeDetector_Tests
    {
        [TestMethod]
        public void WontFireEvent_WhenNotRemoteSession()
        {
            bool homeOfficeStarted = false;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedIsRemoteSession = false;
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsFalse(homeOfficeStarted);
        }

        [TestMethod]
        public void WillFireEvent_WhenInRemoteSessionAndLastInputTodayAfterDawn()
        {
            // it's morning now, 6:30 on June 26th 2016
            Calculations.NowTime = new DateTime(2016, 6, 26, 6, 30, 0);
            bool homeOfficeStarted = false;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(9).TotalMilliseconds; // last input event 1 hour ago
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsTrue(homeOfficeStarted);
        }

        [TestMethod]
        public void WontFireEvent_WhenInRemoteSessionAndLastInputTodayBeforeDawn()
        {
            // it's today's night now, 2:30 on June 26th 2016
            Calculations.NowTime = new DateTime(2016, 6, 26, 2, 30, 0);
            bool homeOfficeStarted = false;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = detector.ForcedEnvironmentTickCount - (uint)TimeSpan.FromSeconds(1).TotalMilliseconds; // last input event just now
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsFalse(homeOfficeStarted);
        }

        [TestMethod]
        public void WontFireEvent_WhenInRemoteSessionButLastInputYesterday()
        {
            // it's morning now, 6:30 on June 26th 2016
            Calculations.NowTime = new DateTime(2016, 6, 26, 6, 30, 0);
            bool homeOfficeStarted = false;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(2).TotalMilliseconds; // last input event 8 hours ago
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsFalse(homeOfficeStarted);
        }

        [TestMethod]
        public void WontFireEvent_WhenLastInputMessedUpByOS()
        {
            // it's morning now, 6:30 on June 26th 2016
            Calculations.NowTime = new DateTime(2016, 6, 26, 6, 30, 0);
            bool homeOfficeStarted = false;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(11).TotalMilliseconds; // impossible last input event!
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsFalse(homeOfficeStarted);
        }

        [TestMethod]
        public void WontFireEvent_WhenTodayFiredAlready()
        {
            // it's morning now, 6:30 on June 26th 2016
            Calculations.NowTime = new DateTime(2016, 6, 26, 6, 30, 0);
            int homeOfficeStarted = 0;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(9).TotalMilliseconds; // last input event 1 hour ago
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted++;

            detector.DetectNow();
            detector.DetectNow();

            Assert.AreEqual(1, homeOfficeStarted);
        }

        [TestMethod]
        public void WillFireEvent_WhenHomeOfficeAndLastFiredYesterday()
        {
            // it's _yesterday_ morning now, 6:30
            Calculations.NowTime = new DateTime(2016, 6, 25, 6, 30, 0);
            int homeOfficeStarted = 0;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(9).TotalMilliseconds; // last input event 1 hour ago
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted++;
            detector.DetectNow();

            // it's _today_ morning now, 6:30
            Calculations.NowTime = new DateTime(2016, 6, 26, 6, 30, 0);
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(34).TotalMilliseconds; // system running for 10+24 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(33).TotalMilliseconds; // last input event 1 hour ago
            detector.DetectNow();

            Assert.AreEqual(2, homeOfficeStarted);
        }

        [TestMethod]
        public void WillFireEvent_WithPrevMinutesTickCount_WhenHomeOfficeActivityDetectedInNextMinuteOnly()
        {
            // it's morning, 6:47 min 10 sec now,
            // but it was    6:46 min 59 sec when we've logged in (and thus made the 1st home-office activity)
            Calculations.NowTime = new DateTime(2016, 6, 25, 6, 47, 10);
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)detector.ForcedEnvironmentTickCount - (uint)TimeSpan.FromSeconds(11).TotalMilliseconds; // last input event 11 seconds earlier, thus in the previous minute
            Fake_HomeOfficeDetector.HomeOfficeActivityEventArgs firedArgs = null;
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => firedArgs = args;

            detector.DetectNow();

            Assert.AreEqual(detector.ForcedLastInputTime, detector.ForcedPublic_tickCountFromDateTime(firedArgs.EventTime));
        }

        [TestMethod]
        public void WillFireEvent_WhenHomeOfficeAfterNightWorkAtWork()
        {
            int homeOfficeStarted = 0;
            var detector = new Fake_HomeOfficeDetector();
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted++;
            // it's late night in work, 0:05 (finishing work 5 mins after midnight! :o)
            Calculations.NowTime = new DateTime(2016, 6, 26, 0, 5, 0);
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = false;
            detector.ForcedLastInputTime = (uint)(TimeSpan.FromHours(10).TotalMilliseconds - 1); // last input event just now
            detector.DetectNow();

            // it's _today_ morning now, 7:05
            Calculations.NowTime = new DateTime(2016, 6, 26, 7, 5, 0);
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(17).TotalMilliseconds; // system running for 10+7 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)(TimeSpan.FromHours(17).TotalMilliseconds - 1); // last input event just now
            detector.DetectNow();

            Assert.AreEqual(1, homeOfficeStarted);
        }

        [TestMethod]
        public void WillFireEvent_WithTimeOfLogon_WhenTimeMergeStartedOnlyAfterUserLoggedOn()
        {
            int homeOfficeStarted = 0;
            var detector = new Fake_HomeOfficeDetector();
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted++;
            // it's morning on a "home office" day, 7:05, when user starts TimeMerge app
            Calculations.NowTime = new DateTime(2016, 10, 31, 7, 5, 0);
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)(TimeSpan.FromHours(10).TotalMilliseconds - 1); // last input event just now
            detector.ForcedMostRecentLogonTime = detector.ForcedEnvironmentTickCount - (uint)TimeSpan.FromMinutes(5).TotalMilliseconds; // logged on 5 minutes before starting TimeMerge
            Fake_HomeOfficeDetector.HomeOfficeActivityEventArgs firedArgs = null;
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => firedArgs = args;

            detector.DetectNow();

            Assert.AreEqual(1, homeOfficeStarted);
            Assert.AreEqual(detector.ForcedMostRecentLogonTime, detector.ForcedPublic_tickCountFromDateTime(firedArgs.EventTime));
        }

        [TestMethod]
        public void WontFireEvent_WithTickCountOverflows()
        {
            bool homeOfficeStarted = false;
            Calculations.NowTime = new DateTime(2016, 6, 25, 6, 47, 10);
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(11).TotalMilliseconds; // last input time _later_ than current system time!!!
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsFalse(homeOfficeStarted);
        }

        [TestMethod]
        public void WontFireEvent_WhenDesktopLocked()
        {
            // it's morning now, 6:30 on June 26th 2016
            Calculations.NowTime = new DateTime(2016, 6, 26, 6, 30, 0);
            bool homeOfficeStarted = false;
            var detector = new Fake_HomeOfficeDetector();
            detector.ForcedEnvironmentTickCount = (uint)TimeSpan.FromHours(10).TotalMilliseconds; // system running for 10 hours
            detector.ForcedIsRemoteSession = true;
            detector.ForcedIsDesktopLocked = true; // Desktop locked now!
            detector.ForcedLastInputTime = (uint)TimeSpan.FromHours(9).TotalMilliseconds; // last input event 1 hour ago
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsFalse(homeOfficeStarted);
        }

        [TestMethod]
        public void RealNumbersExample()
        {
            bool homeOfficeStarted = false;
            Calculations.NowTime = new DateTime(2016, 7, 4, 6, 24, 0);
            var detector = new Fake_HomeOfficeDetector();
            // Environment.TickCount starts at Int32.MinValue, increments towards 0 and then up to Int32.MaxValue
            detector.ForcedEnvironmentTickCount = 60 * 60 * 1000; // system running for 1 hour now
            detector.ForcedIsRemoteSession = true;
            detector.ForcedLastInputTime = 59 * 60 * 1000; // last input time 1 minute ago
            detector.HomeOfficeActivityOfDayDetected += (sender, args) => homeOfficeStarted = true;

            detector.DetectNow();

            Assert.IsTrue(homeOfficeStarted);
        }
    }

    [TestClass]
    public class TickCountConverter_Tests
    {
        [TestMethod]
        public void TickCountFromDateTime_CalculatesTickCount()
        {
            Calculations.NowTime = new DateTime(2016, 10, 30, 21, 49, 5);
            // System was started, say, at 00:00 on 2016-Oct-30
            uint systemUpTimeTickCount = (uint)(TimeSpan.FromHours(21) + TimeSpan.FromMinutes(49) + TimeSpan.FromSeconds(5)).TotalMilliseconds;
            var toConvert = new DateTime(2016, 10, 30, 22, 11, 10);

            uint convertedTickCount = HomeOfficeDetector.TickCountConverter.TickCountFromDateTime(Calculations.NowTime, systemUpTimeTickCount, toConvert);

            uint expectedTickCount = (uint)(TimeSpan.FromHours(22) + TimeSpan.FromMinutes(11) + TimeSpan.FromSeconds(10)).TotalMilliseconds;
            Assert.AreEqual(expectedTickCount, convertedTickCount);
        }

        [TestMethod]
        public void DateTimeFromTickCount_CalculatesDateTime()
        {
            Calculations.NowTime = new DateTime(2016, 10, 30, 21, 49, 5);
            // System was started, say, at 00:00 on 2016-Oct-30
            uint systemUpTimeTickCount = (uint)(TimeSpan.FromHours(21) + TimeSpan.FromMinutes(49) + TimeSpan.FromSeconds(5)).TotalMilliseconds;
            uint toConvertTickCount = (uint)(TimeSpan.FromHours(22) + TimeSpan.FromMinutes(11) + TimeSpan.FromSeconds(10)).TotalMilliseconds;

            DateTime convertedDateTime = HomeOfficeDetector.TickCountConverter.DateTimeFromTickCount(Calculations.NowTime, systemUpTimeTickCount, toConvertTickCount);

            var expectedDateTime = new DateTime(2016, 10, 30, 22, 11, 10);
            Assert.AreEqual(expectedDateTime, convertedDateTime);
        }
    }
}

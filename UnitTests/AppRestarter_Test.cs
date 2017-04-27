using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.Utils;
using Moq;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class AppRestarter_Test
    {
        class Fake_AppRestarter : AppRestarter
        {
            Stopwatch CallsStopwatch = Stopwatch.StartNew();

            public TimeSpan CalledAppStartTimeOffset { get; set; }
            public List<string> CalledAppStartParams { get; set; }
            protected override void appStart(string startParam)
            {
                CalledAppStartTimeOffset = CallsStopwatch.Elapsed;
                CalledAppStartParams = new List<string>(startParam.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries));
                Thread.Sleep(1); // making timing-based verification possible
            }

            public bool CalledAppShutdown { get; set; }
            public TimeSpan CalledAppShutdownTimeOffset { get; set; }
            protected override void appShutdown()
            {
                CalledAppShutdown = true;
                CalledAppShutdownTimeOffset = CallsStopwatch.Elapsed;
                Thread.Sleep(1); // making timing-based verification possible
            }
        }

        [TestMethod]
        public void ShutDown_WillCallAppShutdown_Always()
        {
            var stubSingleInstanceGuard = new Mock<ISingleAppInstanceGuard>();
            var restarter = new Fake_AppRestarter();
            restarter.Init(stubSingleInstanceGuard.Object);

            restarter.ShutDown();

            Assert.IsTrue(restarter.CalledAppShutdown);
        }

        [TestMethod]
        public void ShutDown_WillNotifySingleInstanceGuard_Always()
        {
            var spySingleInstanceGuard = new Mock<ISingleAppInstanceGuard>();
            var restarter = new Fake_AppRestarter();
            restarter.Init(spySingleInstanceGuard.Object);

            restarter.ShutDown();

            spySingleInstanceGuard.Verify(g => g.OnBeforeAppShutDown(), Times.Exactly(1));
        }

        [TestMethod]
        public void ShutDownAndRestart_WillStartNewProcessWithStartArgument_WithoutSpecifyingStartArguments()
        {
            var stubSingleInstanceGuard = new Mock<ISingleAppInstanceGuard>();
            var restarter = new Fake_AppRestarter();
            restarter.Init(stubSingleInstanceGuard.Object);

            restarter.ShutDownAndRestart(null);

            Assert.AreEqual(TimeMerge.App.NightlyRestart, restarter.CalledAppStartParams[0]);
        }

        [TestMethod]
        public void ShutDownAndRestart_WillStartNewProcessWithStartArgument_WhenStartArgumentSpecified()
        {
            var stubSingleInstanceGuard = new Mock<ISingleAppInstanceGuard>();
            var restarter = new Fake_AppRestarter();
            restarter.Init(stubSingleInstanceGuard.Object);

            restarter.ShutDownAndRestart("/dummyParam");

            Assert.AreEqual("/dummyParam", restarter.CalledAppStartParams[0]);
            Assert.AreEqual(TimeMerge.App.NightlyRestart, restarter.CalledAppStartParams[1]);
            Assert.AreEqual(2, restarter.CalledAppStartParams.Count);
        }

        [TestMethod]
        public void ShutDownAndRestart_WillStartNewProcessAfterShuttingDownThisOne_Always()
        {
            var stubSingleInstanceGuard = new Mock<ISingleAppInstanceGuard>();
            var restarter = new Fake_AppRestarter();
            restarter.Init(stubSingleInstanceGuard.Object);

            restarter.ShutDownAndRestart(null);

            Assert.IsTrue(restarter.CalledAppStartTimeOffset != TimeSpan.Zero);
            Assert.IsTrue(restarter.CalledAppShutdownTimeOffset != TimeSpan.Zero);
            Assert.IsTrue(restarter.CalledAppStartTimeOffset > restarter.CalledAppShutdownTimeOffset);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Windows;
using System.Runtime.InteropServices;

namespace UnitTests
{
    [TestClass]
    public class SingleAppInstanceGuard_Test
    {
        class Fake_SingleAppInstanceGuard : TimeMerge.Utils.SingleAppInstanceGuard
        {
            public Fake_SingleAppInstanceGuard()
            {
                ForcedNameOfSystemWideSyncEvent = "--fake-sync-event--{71455225-8162-46C8-80BE-C195A1E818D3}";
            }
            public string ForcedNameOfSystemWideSyncEvent { get; set; }
            protected override string NameOfSystemWideSyncEvent
            {
                get { return ForcedNameOfSystemWideSyncEvent; }
            }
        }

        [TestMethod]
        public void OnAppStartup_WillUnblockWaitingOfPreviousRunningAppInstance_Always()
        {
            ManualResetEvent eventOtherAppStarted = new ManualResetEvent(false);
            var threadOtherAppInstance = new Thread((object instanceStartedRaw) =>
            {
                var otherGuard = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "A--fake-sync-event--" };
                otherGuard.OnAppStartup();
                (instanceStartedRaw as ManualResetEvent).Set();
                otherGuard.WaitForShowRunningInstanceRequest();
                otherGuard.Dispose();
            });
            threadOtherAppInstance.Start(eventOtherAppStarted);
            eventOtherAppStarted.WaitOne();

            var guard = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "A--fake-sync-event--" };
            guard.OnAppStartup();

            // implicit Assert: fail when Join() never happens
            threadOtherAppInstance.Join();
            guard.Dispose();
        }

        [TestMethod]
        public void IsTheOnlyRunningInstance_WillReturnTrue_WhenNoOtherInstanceIsRunning()
        {
            var guard = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "B--fake-sync-event--" };
            guard.OnAppStartup();

            bool isTheOnlyRunning = guard.IsTheOnlyRunningInstance();

            Assert.IsTrue(isTheOnlyRunning);
            guard.Dispose();
        }

        [TestMethod]
        public void IsTheOnlyRunningInstance_WillReturnFalse_WhenOtherInstanceRunningAlready()
        {
            ManualResetEvent eventOtherAppStarted = new ManualResetEvent(false);
            var threadOtherAppInstance = new Thread((object instanceStartedRaw) =>
            {
                var otherGuard = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "C--fake-sync-event--" };
                otherGuard.OnAppStartup();
                (instanceStartedRaw as ManualResetEvent).Set();
                otherGuard.WaitForShowRunningInstanceRequest();
                // otherGuard.Dispose();
            });
            threadOtherAppInstance.Start(eventOtherAppStarted);
            eventOtherAppStarted.WaitOne();
            var guard = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "C--fake-sync-event--" };
            guard.OnAppStartup();

            bool isTheOnlyRunning = guard.IsTheOnlyRunningInstance();

            Assert.IsFalse(isTheOnlyRunning);
            threadOtherAppInstance.Join(); // clean-up
            guard.Dispose();
        }

        [TestMethod]
        public void OnBeforeAppShutDown_WillAllowNextInstanceToBeTheOnlyRunningInstance_Always()
        {
            var guard = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "D--fake-sync-event--" };
            guard.OnAppStartup();
            guard.OnBeforeAppShutDown();

            var guard2 = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "D--fake-sync-event--" };
            guard2.OnAppStartup();

            Assert.IsTrue(guard2.IsTheOnlyRunningInstance());
            guard.Dispose();
            guard2.Dispose();
        }

        [TestMethod]
        public void Dispose_WillAllowNextInstanceToBeTheOnlyRunningInstance_Always()
        {
            using (var guard = new Fake_SingleAppInstanceGuard())
            {
                guard.ForcedNameOfSystemWideSyncEvent = "E--fake-sync-event--";
                guard.OnAppStartup();
            } // will call Dispose() implicitly

            var guard2 = new Fake_SingleAppInstanceGuard() { ForcedNameOfSystemWideSyncEvent = "E--fake-sync-event--" };
            guard2.OnAppStartup();

            Assert.IsTrue(guard2.IsTheOnlyRunningInstance());
            guard2.Dispose();
        }
    }
}

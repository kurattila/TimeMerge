using System;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;

namespace TimeMerge.Utils
{
    public class HomeOfficeDetector
    {
        static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(HomeOfficeDetector));

        internal class TickCountConverter
        {
            public static uint TickCountFromDateTime(DateTime now, uint nowTickCount, DateTime dateTimeToConvert)
            {
                DateTime systemStartupDateTime = now - TimeSpan.FromMilliseconds(nowTickCount);
                return (uint)(dateTimeToConvert - systemStartupDateTime).TotalMilliseconds;
            }

            public static DateTime DateTimeFromTickCount(DateTime now, uint nowTickCount, uint tickCountToConvert)
            {
                DateTime systemStartupDateTime = now - TimeSpan.FromMilliseconds(nowTickCount);
                return systemStartupDateTime + TimeSpan.FromMilliseconds(tickCountToConvert);
            }
        }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public class HomeOfficeActivityEventArgs : EventArgs
        {
            public DateTime EventTime { get; set; }
        }

        public virtual event EventHandler<HomeOfficeActivityEventArgs> HomeOfficeActivityOfDayDetected;

        DateTime dateOfEventFired = new DateTime();
        protected void fireHomeOfficeActivityOfDayDetected(uint tickCountOfHomeOfficeActivityStart)
        {
            if (dateOfEventFired == Calculations.NowTime.Date)
                return;

            uint tickCount = getEnvironmentTickCount();
            uint millisecsSinceLastUserActivity = tickCount - tickCountOfHomeOfficeActivityStart;
            logger.Info(string.Format("fireHomeOfficeActivityOfDayDetected(): tickcount={0}, millisecsSinceLastUserActivity={1}", tickCount, millisecsSinceLastUserActivity));

            if (isDesktopLocked())
            {
                logger.Info(string.Format("Desktop Locked"));
                return;
            }

            if (HomeOfficeActivityOfDayDetected != null)
                HomeOfficeActivityOfDayDetected(this, new HomeOfficeActivityEventArgs() { EventTime = dateTimeFromTickCount(tickCountOfHomeOfficeActivityStart) });
            dateOfEventFired = Calculations.NowTime.Date;
        }

        protected virtual uint getLastInputTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            if (!GetLastInputInfo(ref lastInPut))
                return 0;
            else
                return lastInPut.dwTime;
        }

        protected virtual uint getMostRecentRemoteLogonTime()
        {
            uint mostRecentRemoteLogonTickCount = 0;

            string logonEventID = "1149";
            string RemoteConnectionLogSource = "Microsoft-Windows-TerminalServices-RemoteConnectionManager/Operational";
            string remoteLogonsQuery = "*[System/EventID=" + logonEventID + "]";

            DateTime? mostRecentLogon = null;

            var elQuery = new EventLogQuery(RemoteConnectionLogSource, PathType.LogName, remoteLogonsQuery);
            var elReader = new System.Diagnostics.Eventing.Reader.EventLogReader(elQuery);

            for (EventRecord record = elReader.ReadEvent(); record != null; record = elReader.ReadEvent())
            {
                if (!mostRecentLogon.HasValue || mostRecentLogon.Value < record.TimeCreated)
                    mostRecentLogon = record.TimeCreated;
            }

            if (mostRecentLogon.HasValue)
            {
                uint nowTickCount = getEnvironmentTickCount();
                TimeSpan mostRecentRemoteLogonAge = Utils.Calculations.NowTime - mostRecentLogon.Value;
                mostRecentRemoteLogonTickCount = nowTickCount - (uint)mostRecentRemoteLogonAge.TotalMilliseconds;
            }
            return mostRecentRemoteLogonTickCount;
        }

        protected DateTime dateTimeFromTickCount(uint tickCount)
        {
            return TickCountConverter.DateTimeFromTickCount(Calculations.NowTime, getEnvironmentTickCount(), tickCount);
        }

        protected uint tickCountFromDateTime(DateTime dateTime)
        {
            return TickCountConverter.TickCountFromDateTime(Calculations.NowTime, getEnvironmentTickCount(), dateTime);
        }

        public virtual void DetectNow()
        {
            if (isRemoteSessionActive())
            {
                uint? homeOfficeStart = null;

                var lastInputTime = getLastInputTime();
                if (isAfterTodaysDawn(lastInputTime))
                    homeOfficeStart = lastInputTime;

                var lastRemoteLogonTime = getMostRecentRemoteLogonTime();
                if (isAfterTodaysDawn(lastRemoteLogonTime))
                    homeOfficeStart = lastRemoteLogonTime;

                if (homeOfficeStart.HasValue)
                    fireHomeOfficeActivityOfDayDetected(homeOfficeStart.Value);
            }
        }

        private bool isAfterTodaysDawn(uint inputTime)
        {
            uint tickCount = getEnvironmentTickCount();
            var now = Calculations.NowTime;
            var dawnTime = new DateTime(now.Year, now.Month, now.Day, 4, 30, 0);
            var inputDateTime = now - TimeSpan.FromMilliseconds(tickCount - inputTime);

            bool lastInputFromToday = dawnTime < inputDateTime;
            return lastInputFromToday;
        }

        // Gets the number of _milliseconds_ elapsed since the system started.
        protected virtual uint getEnvironmentTickCount()
        {
            // MSDN:
            // Because the value of the TickCount property value is a 32-bit signed integer, if the system runs continuously,
            // TickCount will increment from zero to Int32.MaxValue for approximately 24.9 days, then jump to Int32.MinValue,
            // which is a negative number, then increment back to zero during the next 24.9 days.
            return (uint)Environment.TickCount; // converting to uint will correctly simulate a growing uint value from 0 to UInt32.MaxValue
        }

        protected virtual bool isRemoteSessionActive()
        {
            return System.Windows.Forms.SystemInformation.TerminalServerSession;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

        protected virtual bool isDesktopLocked()
        {
            var handle = OpenInputDesktop(0, false, 0);
            bool locked = (handle == IntPtr.Zero);
            return locked;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeMerge.Utils;
using System.Threading;
using System.Globalization;

namespace UnitTests
{
    [TestClass()]
    public class UtilsCalculationsTest
    {
        [TestMethod()]
        public void DateTimeToHourMinuteString_UsesInvariantCulture_Always()
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            DateTime dateTime = new DateTime(2016, 2, 18, 7, 7, 7);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("sk");
            string textualRepresentation = Calculations.DateTimeToHourMinuteString(dateTime);

            Assert.AreEqual("07:07", textualRepresentation); // "sk" culture would not have a leading zero for hours: "7:07"
            Thread.CurrentThread.CurrentCulture = oldCulture;
        }

        /////////////////////////////////////
        /// MonthBalanceAsHumanString
        /////////////////////////////////////
        [TestMethod()]
        public void MonthBalanceAsHumanString_FormatsAsHoursAndMinutes()
        {
            TimeSpan fifteenForty = TimeSpan.FromMinutes(15 * 60 + 40);

            string formatted = Calculations.MonthBalanceAsHumanString(fifteenForty);

            Assert.AreEqual("+15:40", formatted);
        }

        [TestMethod()]
        public void MonthBalanceAsHumanString_HasLeadingZeroForMinutes()
        {
            TimeSpan twoHoursTwoMinutes = TimeSpan.FromMinutes(2 * 60 + 2);

            string formatted = Calculations.MonthBalanceAsHumanString(twoHoursTwoMinutes);

            Assert.AreEqual("+2:02", formatted);
        }

        [TestMethod()]
        public void MonthBalanceAsHumanString_FormatsNegativeTime_WithoutNegativeMinsSign()
        {
            TimeSpan twoHoursTwoMinutes = TimeSpan.FromMinutes(-2);

            string formatted = Calculations.MonthBalanceAsHumanString(twoHoursTwoMinutes);

            Assert.AreEqual("-0:02", formatted); //"-0:-02" would be wrong
        }

        [TestMethod()]
        public void MonthBalanceAsHumanString_Handles_MoreThan24Hours()
        {
            TimeSpan oneFullDayAndFortyMinutes = TimeSpan.FromMinutes(24 * 60 + 40);

            string formatted = Calculations.MonthBalanceAsHumanString(oneFullDayAndFortyMinutes);

            Assert.AreEqual("+24:40", formatted);
        }



        /////////////////////////////////////
        /// ParseStringToTimeSpan
        /////////////////////////////////////
        [TestMethod()]
        public void ParseStringToTimeSpan_NormalTimeGetsParsed()
        {
            string timeSpanString = "2:02";

            TimeSpan timeSpan = Calculations.ParseHHMMToTimeSpan(timeSpanString);

            Assert.AreEqual(TimeSpan.FromMinutes(2 * 60 + 2), timeSpan);
        }

        [TestMethod()]
        public void ParseStringToTimeSpan_NegativeTimeGetsParsed()
        {
            string timeSpanString = "-12:02";

            TimeSpan timeSpan = Calculations.ParseHHMMToTimeSpan(timeSpanString);

            Assert.AreEqual(TimeSpan.FromMinutes(-12 * 60 - 2), timeSpan);
        }

        [TestMethod()]
        public void ParseStringToTimeSpan_NegativeTimeGetsParsed_WhenHoursPartIsZero()
        {
            string timeSpanString = "-0:02";

            TimeSpan timeSpan = Calculations.ParseHHMMToTimeSpan(timeSpanString);

            Assert.AreEqual(TimeSpan.FromMinutes(-2), timeSpan);
        }

        [TestMethod()]
        public void ParseStringToTimeSpan_PositiveTimeGetsParsed_WhenHoursPartIsZero()
        {
            string timeSpanString = "0:02";

            TimeSpan timeSpan = Calculations.ParseHHMMToTimeSpan(timeSpanString);

            Assert.AreEqual(TimeSpan.FromMinutes(2), timeSpan);
        }

        [TestMethod()]
        public void ParseStringToTimeSpan_MoreThan24HourseGetParsed()
        {
            string timeSpanString = "25:02";

            TimeSpan timeSpan = Calculations.ParseHHMMToTimeSpan(timeSpanString);

            Assert.AreEqual(TimeSpan.FromMinutes(25 * 60 + 2), timeSpan);
        }
    }
}

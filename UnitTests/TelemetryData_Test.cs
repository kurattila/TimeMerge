using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class TelemetryData_Test
    {
        [TestMethod]
        public void Serialize_ReturnsEmptyString_ByDefault()
        {
            var data = new TimeMerge.Utils.TelemetryData();

            string serializedData = data.Serialize();

            Assert.AreEqual(0, serializedData.Length);
        }

        [TestMethod]
        public void Serialize_ReturnsSingleValue_WhenSingleEntryMadeOnly()
        {
            var data = new TimeMerge.Utils.TelemetryData();
            data.Add("dummyKey", "dummyValue");

            string serializedData = data.Serialize();

            Assert.AreEqual("dummyKey=dummyValue", serializedData);
        }

        [TestMethod]
        public void Serialize_ReturnsTwoValues_WhenTwoEntriesMade()
        {
            var data = new TimeMerge.Utils.TelemetryData();
            data.Add("dummyKey", "dummyValue");
            data.Add("dummyKey2", "dummyValue2");

            string serializedData = data.Serialize();

            Assert.AreEqual("dummyKey=dummyValue|dummyKey2=dummyValue2", serializedData);
        }

        [TestMethod]
        public void Serialize_Returns5Values_When5EntriesMade()
        {
            var data = new TimeMerge.Utils.TelemetryData();
            data.Add("one", "1");
            data.Add("two", "2");
            data.Add("three", "3");
            data.Add("four", "4");
            data.Add("five", "5");

            string serializedData = data.Serialize();

            Assert.AreEqual("one=1|two=2|three=3|four=4|five=5", serializedData);
        }
    }
}

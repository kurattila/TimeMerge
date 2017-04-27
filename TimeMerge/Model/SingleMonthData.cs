using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;

namespace TimeMerge.Model
{
    public class SingleMonthData
    {
        public SingleMonthData()
        {
            this.Days = new List<SingleDayData>();
        }

        public DateTime YearMonth { get; set; }
        public TimeSpanSerializable TransferFromPrevMonth { get; set; }
        public List<SingleDayData> Days { get; set; }

        /// <summary>
        /// Just force XML Serializer to save a "month title" as well (so that XSLT does not need to show only "2016-04", but a more readable "2016 April" instead)
        /// </summary>
        public string CachedTitleOfMonth
        {
            get
            {
                return YearMonth.ToString("MMMM yyyy");
            }
            set
            {
                // on reading from XML, just ignore this value
            }
        }
        public string CachedBalanceOfMonth { get; set; }
    }

    /// <summary>
    /// A TimeSpan implementation that is actually serializable to XML
    /// Modified, based on http://stackoverflow.com/questions/637933/net-how-to-serialize-a-timespan-to-xml
    /// </summary>
    public struct TimeSpanSerializable : IXmlSerializable
    {
        private System.TimeSpan _value;

        public static implicit operator TimeSpanSerializable(System.TimeSpan value)
        {
            return new TimeSpanSerializable { _value = value };
        }

        public static implicit operator System.TimeSpan(TimeSpanSerializable value)
        {
            return value._value;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                _value = System.TimeSpan.Parse(reader.ReadContentAsString());
            }
            catch(System.InvalidOperationException)
            {
                // 'ReadContentAsString()' method above is not supported on node type Element,
                // thus we might sometimes need to use 'ReadElementContentAsString()' instead
                _value = System.TimeSpan.Parse(reader.ReadElementContentAsString());
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(_value.ToString());
        }
    }
}

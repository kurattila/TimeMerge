using System;
using System.Collections.Generic;
using System.Text;

namespace TimeMerge.Utils
{
    internal class TelemetryData
    {
        List<Tuple<string, string>> _Data = new List<Tuple<string, string>>();

        public void Add(string key, string value)
        {
            _Data.Add(new Tuple<string, string>(key, value));
        }

        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var pair in _Data)
            {
                if (sb.ToString().Length > 0)
                    sb.Append("|");
                sb.Append(string.Format("{0}={1}", pair.Item1, pair.Item2));
            }

            return sb.ToString();
        }
    }
}

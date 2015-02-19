using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RestSharp.Portable.Deserializers;

namespace RestSharp.Portable.Test
{
    internal class DictionaryDeserializer : IDeserializer
    {
        public DictionaryDeserializer()
        {
            Encoding = Encoding.UTF8;
        }

        public Encoding Encoding { get; private set; }

        public T Deserialize<T>(IRestResponse response)
        {
            var text = Encoding.ASCII.GetString(response.RawBytes);

            var kvp = from line in text.Split('&')
                      let parts = line.Split(new[] { '=' }, 2)
                      let key = Decode(parts[0].Trim())
                      let value = parts.Length > 1 ? Decode(parts[1]) : null
                      select new { key, value };

            object result;
            if (typeof(T) == typeof(IDictionary<string, string>)
                || typeof(T) == typeof(Dictionary<string, string>))
            {
                result = kvp.ToDictionary(x => x.key, x => x.value);
            }
            else
                throw new NotSupportedException();

            return (T)result;
        }

        private string Decode(string s)
        {
            s = Uri.UnescapeDataString(s).Replace('+', ' ');
            s = Encoding.GetString(s.Select(x => (byte)x).ToArray());
            return s;
        }
    }
}

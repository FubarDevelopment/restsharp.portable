using RestSharp.Portable.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Test
{
    class DictionaryDeserializer : IDeserializer
    {
        public Encoding Encoding { get; private set; }

        public DictionaryDeserializer()
        {
            Encoding = System.Text.Encoding.UTF8;
        }

        private string Decode(string s)
        {
            s = Uri.UnescapeDataString(s).Replace('+', ' ');
            s = Encoding.GetString(s.Select(x => (byte)x).ToArray());
            return s;
        }

        public T Deserialize<T>(IRestResponse response)
        {
            var text = System.Text.Encoding.ASCII.GetString(response.RawBytes);

            var kvp = from line in text.Split('&')
                      let parts = line.Split(new[] { '=' }, 2)
                      let key = Decode(parts[0].Trim())
                      let value = (parts.Length > 1 ? Decode(parts[1]) : null)
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
    }
}

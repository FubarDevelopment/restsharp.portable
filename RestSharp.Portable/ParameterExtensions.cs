using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable
{
    static class ParameterExtensions
    {
        private static readonly string _alphanum = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string _mark = "-_.!~*'()";

        private static readonly IDictionary<byte, bool> _allowedBytes = DefaultEncoding.GetBytes(_alphanum + _mark)
            .ToDictionary(x => x, x => true);
        private static readonly IDictionary<char, bool> _allowedChars = (_alphanum + _mark).Cast<char>()
            .ToDictionary(x => x, x => true);

        internal static readonly Encoding DefaultEncoding = Encoding.UTF8;

        internal static string UrlDecode(string s)
        {
            return Uri.UnescapeDataString(s.Replace('+', ' '));
        }

        internal static string UrlEncode(string s, Encoding encoding, bool spaceAsPlus)
        {
            var buffer = new char[1];
            var result = new StringBuilder();
            foreach (var ch in s)
            {
                if (spaceAsPlus && ch == ' ')
                    result.Append('+');
                else if (_allowedChars.ContainsKey(ch))
                    result.Append(ch);
                else
                {
                    buffer[0] = ch;
                    var data = encoding.GetBytes(buffer);
                    foreach (var v in data)
                        result.AppendFormat("%{0:x2}", v);
                }
            }
            return result.ToString();
        }

        internal static string UrlEncode(byte[] v, bool spaceAsPlus)
        {
            var result = new StringBuilder();
            foreach (var b in v)
            {
                if (spaceAsPlus && b == 32)
                    result.Append('+');
                else if (_allowedBytes.ContainsKey(b))
                {
                    result.Append((char)b);
                }
                else
                    result.AppendFormat("%{0:x2}", b);
            }
            return result.ToString();
        }

        internal static string UrlEncode(object v, Encoding encoding, bool spaceAsPlus)
        {
            if (v == null)
                return string.Empty;
            if (v is string)
                return UrlEncode((string)v, encoding, spaceAsPlus);
            if (v is byte[])
                return UrlEncode((byte[])v, spaceAsPlus);
            return UrlEncode(string.Format("{0}", v), encoding, spaceAsPlus);
        }

        internal static string ToEncodedString(this Parameter parameter, IRestRequest request, bool spaceAsPlus = false)
        {
            switch (parameter.Type)
            {
                case ParameterType.GetOrPost:
                case ParameterType.QueryString:
                case ParameterType.UrlSegment:
                    return UrlEncode(parameter.Value, parameter.Encoding ?? DefaultEncoding, spaceAsPlus);
            }
            throw new NotSupportedException(string.Format("Parameter of type {0} doesn't support an encoding.", parameter.Type));
        }
    }
}

#region License
// Copyright 2010 John Sheehan
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth.Extensions
{
    internal static class StringExtensions
    {
        public static bool IsNullOrBlank(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }
        public static bool EqualsIgnoreCase(this string left, string right)
        {
            return String.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;
        }
        public static bool EqualsAny(this string input, params string[] args)
        {
            return args.Aggregate(false, (current, arg) => current | input.Equals(arg));
        }
        public static string FormatWith(this string format, params object[] args)
        {
            return String.Format(format, args);
        }
        public static string FormatWithInvariantCulture(this string format, params object[] args)
        {
            return String.Format(CultureInfo.InvariantCulture, format, args);
        }
        public static string Then(this string input, string value)
        {
            return String.Concat(input, value);
        }
        public static string UrlEncode(this string value)
        {
            // [DC] This is more correct than HttpUtility; it escapes spaces as %20, not +
            return Uri.EscapeDataString(value);
        }
        public static string UrlDecode(this string value)
        {
            return Uri.UnescapeDataString(value);
        }
        public static Uri AsUri(this string value)
        {
            return new Uri(value);
        }
        public static string ToBase64String(this byte[] input)
        {
            return Convert.ToBase64String(input);
        }
        public static byte[] GetBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }
        public static string PercentEncode(this string s)
        {
            var bytes = s.GetBytes();
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                // [DC]: Support proper encoding of special characters (\n\r\t\b)
                if ((b > 7 && b < 11) || b == 13)
                {
                    sb.Append(string.Format("%0{0:X}", b));
                }
                else
                {
                    sb.Append(string.Format("%{0:X}", b));
                }
            }
            return sb.ToString();
        }
        public static IDictionary<string, string> ParseQueryString(this string query)
        {
            // [DC]: This method does not URL decode, and cannot handle decoded input
            if (query.StartsWith("?")) query = query.Substring(1);
            if (query.Equals(string.Empty))
            {
                return new Dictionary<string, string>();
            }
            var parts = query.Split('&');
            return (from part in parts
                let equalIndex = part.IndexOf('=')
                let name = (equalIndex == -1 ? part : part.Substring(0, equalIndex))
                let value = (equalIndex == -1 ? string.Empty : part.Substring(equalIndex + 1))
                select new KeyValuePair<string, string>(name, value))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}

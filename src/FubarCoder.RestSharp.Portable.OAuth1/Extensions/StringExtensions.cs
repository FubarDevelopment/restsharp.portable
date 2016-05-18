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
using System.Linq;

namespace RestSharp.Portable.OAuth1.Extensions
{
    internal static class StringExtensions
    {
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

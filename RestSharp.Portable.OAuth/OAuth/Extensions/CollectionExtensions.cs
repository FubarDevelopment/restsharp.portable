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
using System.Text;

namespace RestSharp.Portable.Authenticators.OAuth.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            return new[] { item };
        }
        public static IEnumerable<T> And<T>(this T item, T other)
        {
            return new[] { item, other };
        }
        public static IEnumerable<T> And<T>(this IEnumerable<T> items, T item)
        {
            foreach (var i in items)
            {
                yield return i;
            }
            yield return item;
        }
        public static K TryWithKey<T, K>(this IDictionary<T, K> dictionary, T key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(K);
        }
        public static IEnumerable<T> ToEnumerable<T>(this object[] items) where T : class
        {
            foreach (var item in items)
            {
                var record = item as T;
                yield return record;
            }
        }
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
        public static string Concatenate(this WebParameterCollection collection, string separator, string spacer)
        {
            var sb = new StringBuilder();
            var total = collection.Count;
            var count = 0;
            foreach (var item in collection)
            {
                sb.Append(item.Name);
                sb.Append(separator);
                sb.Append(item.Value);
                count++;
                if (count < total)
                {
                    sb.Append(spacer);
                }
            }
            return sb.ToString();
        }
    }
}

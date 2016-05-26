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

namespace RestSharp.Portable.OAuth1.Extensions
{
    internal static class TimeExtensions
    {
        public static DateTime FromNow(this TimeSpan value)
        {
            return new DateTime((DateTime.Now + value).Ticks);
        }
        public static DateTime FromUnixTime(this long seconds)
        {
            var time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(seconds);
            return time.ToLocalTime();
        }
        public static long ToUnixTime(this DateTime dateTime)
        {
            var timeSpan = dateTime - new DateTime(1970, 1, 1);
            var timestamp = (long)timeSpan.TotalSeconds;
            return timestamp;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace RestSharp.Portable
{
    /// <summary>
    /// Authentication/authorization header information
    /// </summary>
    public class AuthHeaderInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthHeaderInfo" /> class.
        /// </summary>
        /// <param name="name">The authentication method name</param>
        /// <param name="rawValue">The raw authentication method values</param>
        /// <param name="values">The parsed authentication method values</param>
        /// <param name="rawValues">The raw parsed authentication method values</param>
        public AuthHeaderInfo(string name, string rawValue, IEnumerable<KeyValuePair<string, string>> values, IEnumerable<KeyValuePair<string, string>> rawValues)
        {
            Name = name;
            RawValue = rawValue;
            Values = values.ToLookup(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            RawValues = rawValues.ToLookup(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the authorization/authentication method name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the authorization/authentication method information
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the parsed authorization/authentication method information
        /// </summary>
        public ILookup<string, string> Values { get; private set; }

        /// <summary>
        /// Gets the raw parsed authorization/authentication method information
        /// </summary>
        public ILookup<string, string> RawValues { get; private set; }
    }
}

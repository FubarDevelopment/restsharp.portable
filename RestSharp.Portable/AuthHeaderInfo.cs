using System;

namespace RestSharp.Portable
{
    /// <summary>
    /// Authentication/authorization header information
    /// </summary>
    public class AuthHeaderInfo
    {
        private static readonly char[] _whiteSpaceCharacters = { ' ', '\t' };

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthHeaderInfo" /> class.
        /// </summary>
        /// <param name="headerValue">The header value to parse</param>
        public AuthHeaderInfo(string headerValue)
        {
            var firstWhitespace = headerValue.IndexOfAny(_whiteSpaceCharacters);
            if (firstWhitespace == -1)
            {
                Name = headerValue;
                Info = null;
            }
            else
            {
                Name = headerValue.Substring(0, firstWhitespace);
                Info = headerValue.Substring(firstWhitespace).TrimStart();
            }
        }

        /// <summary>
        /// Gets the authorization/authentication method name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the authorization/authentication method information
        /// </summary>
        public string Info { get; private set; }
    }
}

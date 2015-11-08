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
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using RestSharp.Portable.Authenticators.OAuth.SignatureProviders;

namespace RestSharp.Portable.Authenticators.OAuth
{
    using Extensions;

    internal static class OAuthTools
    {
        private const string _digit = "1234567890";

        private const string _lower = "abcdefghijklmnopqrstuvwxyz";

        private static readonly UrlEscapeUtility _escapeUtility = new UrlEscapeUtility(false);

        /// <summary>
        /// All text parameters are UTF-8 encoded (per section 5.1).
        /// </summary>
        /// <a href="http://www.hueniverse.com/hueniverse/2008/10/beginners-gui-1.html"/>
        private static readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        /// <a href="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
        private static readonly string[] _uriRfc3986CharsToEscape = { "!", "*", "'", "(", ")" };

        private static readonly string[] _uriRfc3968EscapedHex = { "%21", "%2A", "%27", "%28", "%29" };

        /// <summary>
        /// Gets or sets the random number generator (can be changed for tests)
        /// </summary>
        public static IRandom DefaultRandomNumberGenerator { get; internal set; }

        static OAuthTools()
        {
            DefaultRandomNumberGenerator = new DefaultRandom();
        }

        /// <summary>
        /// Generates a random 16-byte lowercase alphanumeric string.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#nonce"/>
        /// <returns></returns>
        public static string GetNonce([NotNull] IRandom random)
        {
            const string chars = (_lower + _digit);
            var nonce = random.Next(0, chars.Length, 16).Select(n => chars[n]).ToArray();
            return new string(nonce);
        }

        /// <summary>
        /// Generates a timestamp based on the current elapsed seconds since '01/01/1970 0000 GMT"
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#nonce"/>
        /// <returns></returns>
        public static string GetTimestamp()
        {
            return GetTimestamp(DateTime.UtcNow);
        }

        /// <summary>
        /// Generates a timestamp based on the elapsed seconds of a given time since '01/01/1970 0000 GMT"
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#nonce"/>
        /// <param name="dateTime">A specified point in time.</param>
        /// <returns></returns>
        public static string GetTimestamp(DateTime dateTime)
        {
            var timestamp = dateTime.ToUnixTime();
            return timestamp.ToString();
        }

        /// <summary>
        /// URL encodes a string based on section 5.1 of the OAuth spec.
        /// Namely, percent encoding with [RFC3986], avoiding unreserved characters,
        /// upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>
        /// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
        /// RFC 3986 behavior if certain elements are present in a .config file. Even if this
        /// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
        /// host actually having this configuration element present.
        /// </remarks>
        /// <a href="http://oauth.net/core/1.0#encoding_parameters" />
        /// <a href="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
        public static string UrlEncodeRelaxed(string value)
        {
            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            var escaped = new StringBuilder(UrlUtility.Escape(value));
            // Upgrade the escaping to RFC 3986, if necessary.
            for (int i = 0; i < _uriRfc3986CharsToEscape.Length; i++)
            {
                string t = _uriRfc3986CharsToEscape[i];
                escaped.Replace(t, _uriRfc3968EscapedHex[i]);
            }
            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }

        /// <summary>
        /// URL encodes a string based on section 5.1 of the OAuth spec.
        /// Namely, percent encoding with [RFC3986], avoiding unreserved characters,
        /// upper-casing hexadecimal characters, and UTF-8 encoding for text value pairs.
        /// </summary>
        /// <param name="value"></param>
        /// <a href="http://oauth.net/core/1.0#encoding_parameters" />
        public static string UrlEncodeStrict(string value)
        {
            // [JD]: We need to escape the apostrophe as well or the signature will fail
            return _escapeUtility.Escape(value, _encoding, UrlEscapeFlags.AllowAllUnreserved);
        }

        /// <summary>
        /// Sorts a collection of key-value pairs by name, and then value if equal,
        /// concatenating them into a single string. This string should be encoded
        /// prior to, or after normalization is run.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.1.1"/>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string NormalizeRequestParameters(WebParameterCollection parameters)
        {
            var copy = SortParametersExcludingSignature(parameters);
            var concatenated = copy.Concatenate("=", "&");
            return concatenated;
        }

        /// <summary>
        /// Sorts a <see cref="WebParameterCollection"/> by name, and then value if equal.
        /// </summary>
        /// <param name="parameters">A collection of parameters to sort</param>
        /// <returns>A sorted parameter collection</returns>
        public static WebParameterCollection SortParametersExcludingSignature(WebParameterCollection parameters)
        {
            var copy = new WebParameterCollection(parameters.Select(x => new WebParameter(x.Name, x.Value, x.Type)));
            var exclusions = copy.Where(n => string.Equals(n.Name, "oauth_signature", StringComparison.OrdinalIgnoreCase));
            copy.RemoveAll(exclusions);
            copy.ForEach(p =>
            {
                p.Name = UrlEncodeStrict(p.Name);
                if (p.Type == WebParameterType.Query)
                {
                    // Parameter provided by the user
                    p.Value = _escapeUtility.Escape(p.Value, _encoding, UrlEscapeFlags.AllowLikeEscapeUriString);
                }
                else
                {
                    // Authorization or POST parameter
                    p.Value = UrlEncodeStrict(p.Value);
                }
            });
            copy.Sort(
            (x, y) =>
            string.CompareOrdinal(x.Name, y.Name) != 0
            ? string.CompareOrdinal(x.Name, y.Name)
            : string.CompareOrdinal(x.Value, y.Value));
            return copy;
        }

        /// <summary>
        /// Creates a request URL suitable for making OAuth requests.
        /// Resulting URLs must exclude port 80 or port 443 when accompanied by HTTP and HTTPS, respectively.
        /// Resulting URLs must be lower case.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.1.2"/>
        /// <param name="url">The original request URL</param>
        /// <returns></returns>
        public static string ConstructRequestUrl(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            var sb = new StringBuilder();
            var requestUrl = string.Format("{0}://{1}", url.Scheme, url.Host);
            var qualified = string.Format(":{0}", url.Port);
            var basic = url.Scheme == "http" && url.Port == 80;
            var secure = url.Scheme == "https" && url.Port == 443;
            sb.Append(requestUrl);
            sb.Append(!basic && !secure ? qualified : string.Empty);
            sb.Append(url.AbsolutePath);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a request elements concatentation value to send with a request.
        /// This is also known as the signature base.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.1.3"/>
        /// <a href="http://oauth.net/core/1.0#sig_base_example"/>
        /// <param name="method">The request's HTTP method type</param>
        /// <param name="url">The request URL</param>
        /// <param name="parameters">The request's parameters</param>
        /// <returns>A signature base string</returns>
        public static string ConcatenateRequestElements(string method, string url, WebParameterCollection parameters)
        {
            var sb = new StringBuilder();

            // Separating &'s are not URL encoded
            var requestMethod = string.Concat(method.ToUpper(), "&");
            var requestUrl = string.Concat(UrlEncodeRelaxed(ConstructRequestUrl(new Uri(url))), "&");
            var requestParameters = UrlEncodeRelaxed(NormalizeRequestParameters(parameters));
            sb.Append(requestMethod);
            sb.Append(requestUrl);
            sb.Append(requestParameters);
            return sb.ToString();
        }

        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret.
        /// This method is used when the token secret is currently unknown.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureProvider">The hashing method</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer key</param>
        /// <returns></returns>
        public static string GetSignature(ISignatureProvider signatureProvider, string signatureBase, string consumerSecret)
        {
            return GetSignature(signatureProvider, OAuthSignatureTreatment.Escaped, signatureBase, consumerSecret, null);
        }

        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret.
        /// This method is used when the token secret is currently unknown.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureProvider">The hashing method</param>
        /// <param name="signatureTreatment">The treatment to use on a signature value</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer key</param>
        /// <returns></returns>
        public static string GetSignature(ISignatureProvider signatureProvider, OAuthSignatureTreatment signatureTreatment, string signatureBase, string consumerSecret)
        {
            return GetSignature(signatureProvider, signatureTreatment, signatureBase, consumerSecret, null);
        }

        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret and a known token secret.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureProvider">The hashing method</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns></returns>
        public static string GetSignature(ISignatureProvider signatureProvider, string signatureBase, string consumerSecret, string tokenSecret)
        {
            return GetSignature(signatureProvider, OAuthSignatureTreatment.Escaped, signatureBase, consumerSecret, tokenSecret);
        }

        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret and a known token secret.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureProvider">The hashing method</param>
        /// <param name="signatureTreatment">The treatment to use on a signature value</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns></returns>
        public static string GetSignature(
            ISignatureProvider signatureProvider,
            OAuthSignatureTreatment signatureTreatment,
            string signatureBase,
            string consumerSecret,
            string tokenSecret)
        {
            if (string.IsNullOrEmpty(tokenSecret))
            {
                tokenSecret = string.Empty;
            }
            consumerSecret = UrlEncodeRelaxed(consumerSecret);
            tokenSecret = UrlEncodeRelaxed(tokenSecret);
            var data = _encoding.GetBytes(signatureBase);
            var hash = signatureProvider.CalculateSignature(data, consumerSecret, tokenSecret);
            var signature = hash;

            var result = signatureTreatment == OAuthSignatureTreatment.Escaped
                             ? UrlEncodeRelaxed(signature)
                             : signature;
            return result;
        }

        private class DefaultRandom : IRandom
        {
            private static readonly object _randomLock = new object();

            private readonly Random _random;

            public DefaultRandom()
            {
                var key = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
                var seed = BitConverter.ToInt32(key, 0);
                _random = new Random(seed);
            }

            /// <summary>
            /// Gets the next random value with <paramref name="minValue"/> &lt;= n &lt; <paramref name="maxValue"/>
            /// </summary>
            /// <param name="minValue">The minimum value (inclusive)</param>
            /// <param name="maxValue">The maximum value (exclusive)</param>
            /// <returns>the next random value</returns>
            public int Next(int minValue, int maxValue)
            {
                lock (_randomLock)
                    return _random.Next(minValue, maxValue);
            }

            /// <summary>
            /// Gets the next <paramref name="count"/> random values with <paramref name="minValue"/> &lt;= n &lt; <paramref name="maxValue"/>
            /// </summary>
            /// <param name="minValue">The minimum value (inclusive)</param>
            /// <param name="maxValue">The maximum value (exclusive)</param>
            /// <param name="count">The number of random values to generate</param>
            /// <returns>the next random values</returns>
            public int[] Next(int minValue, int maxValue, int count)
            {
                var result = new int[count];
                lock (_randomLock)
                {
                    for (int i = 0; i != count; ++i)
                        result[i] = _random.Next(minValue, maxValue);
                }
                return result;
            }
        }
    }
}

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

namespace RestSharp.Portable.Authenticators.OAuth
{
    using Extensions;

    internal static class OAuthTools
    {
        private const string _digit = "1234567890";
        private const string _lower = "abcdefghijklmnopqrstuvwxyz";
        private const string _upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string _alphaNumeric = _upper + _lower + _digit;
        private const string _unreserved = _alphaNumeric + "-._~";
        private static readonly Random _random;
        private static readonly object _randomLock = new object();

        static OAuthTools()
        {
            var key = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
            var seed = BitConverter.ToInt32(key, 0);
            _random = new Random(seed);
        }
        /// <summary>
        /// All text parameters are UTF-8 encoded (per section 5.1).
        /// </summary>
        /// <a href="http://www.hueniverse.com/hueniverse/2008/10/beginners-gui-1.html"/>
        private static readonly Encoding _encoding = Encoding.UTF8;
        /// <summary>
        /// Generates a random 16-byte lowercase alphanumeric string.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#nonce"/>
        /// <returns></returns>
        public static string GetNonce()
        {
            const string chars = (_lower + _digit);
            var nonce = new char[16];
            lock (_randomLock)
            {
                for (var i = 0; i < nonce.Length; i++)
                {
                    nonce[i] = chars[_random.Next(0, chars.Length)];
                }
            }
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
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        /// <a href="http://stackoverflow.com/questions/846487/how-to-get-uri-escapedatastring-to-comply-with-rfc-3986" />
        private static readonly string[] _uriRfc3986CharsToEscape = { "!", "*", "'", "(", ")" };
        private static readonly string[] _uriRfc3968EscapedHex = { "%21", "%2A", "%27", "%28", "%29" };
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
            var original = value;
            var ret = original.ToCharArray()
                .Where(c => _unreserved.IndexOf(c) == -1 && c != '%')
                .Aggregate(value, (current, c) => current.Replace(c.ToString(), c.ToString().PercentEncode()));
            return ret.Replace("%%", "%25%"); // Revisit to encode actual %'s
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
            var copy = new WebParameterCollection(parameters);
            var exclusions = copy.Where(n => n.Name.EqualsIgnoreCase("oauth_signature"));
            copy.RemoveAll(exclusions);
            copy.ForEach(p => { p.Name = UrlEncodeStrict(p.Name); p.Value = UrlEncodeStrict(p.Value); });
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
            var requestUrl = "{0}://{1}".FormatWith(url.Scheme, url.Host);
            var qualified = ":{0}".FormatWith(url.Port);
            var basic = url.Scheme == "http" && url.Port == 80;
            var secure = url.Scheme == "https" && url.Port == 443;
            sb.Append(requestUrl);
            sb.Append(!basic && !secure ? qualified : "");
            sb.Append(url.AbsolutePath);
            return sb.ToString(); //.ToLower();
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
            var requestMethod = method.ToUpper().Then("&");
            var requestUrl = UrlEncodeRelaxed(ConstructRequestUrl(url.AsUri())).Then("&");
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
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer key</param>
        /// <returns></returns>
        public static string GetSignature(OAuthSignatureMethod signatureMethod, string signatureBase, string consumerSecret)
        {
            return GetSignature(signatureMethod, OAuthSignatureTreatment.Escaped, signatureBase, consumerSecret, null);
        }
        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret.
        /// This method is used when the token secret is currently unknown.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureTreatment">The treatment to use on a signature value</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer key</param>
        /// <returns></returns>
        public static string GetSignature(OAuthSignatureMethod signatureMethod, OAuthSignatureTreatment signatureTreatment, string signatureBase, string consumerSecret)
        {
            return GetSignature(signatureMethod, signatureTreatment, signatureBase, consumerSecret, null);
        }
        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret and a known token secret.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns></returns>
        public static string GetSignature(OAuthSignatureMethod signatureMethod, string signatureBase, string consumerSecret, string tokenSecret)
        {
            return GetSignature(signatureMethod, OAuthSignatureTreatment.Escaped, consumerSecret, tokenSecret);
        }
        /// <summary>
        /// Creates a signature value given a signature base and the consumer secret and a known token secret.
        /// </summary>
        /// <a href="http://oauth.net/core/1.0#rfc.section.9.2"/>
        /// <param name="signatureMethod">The hashing method</param>
        /// <param name="signatureTreatment">The treatment to use on a signature value</param>
        /// <param name="signatureBase">The signature base</param>
        /// <param name="consumerSecret">The consumer secret</param>
        /// <param name="tokenSecret">The token secret</param>
        /// <returns></returns>
        public static string GetSignature(OAuthSignatureMethod signatureMethod,
        OAuthSignatureTreatment signatureTreatment,
        string signatureBase,
        string consumerSecret,
        string tokenSecret)
        {
            if (tokenSecret.IsNullOrBlank())
            {
                tokenSecret = String.Empty;
            }
            consumerSecret = UrlEncodeRelaxed(consumerSecret);
            tokenSecret = UrlEncodeRelaxed(tokenSecret);
            var key = "{0}&{1}".FormatWith(consumerSecret, tokenSecret);
            string signature;
            switch (signatureMethod)
            {
                case OAuthSignatureMethod.HmacSha1:
                {
                    var keyData = _encoding.GetBytes(key);
#if USE_BOUNCYCASTLE
                    var digest = new Org.BouncyCastle.Crypto.Digests.Sha1Digest();
                    var crypto = new Org.BouncyCastle.Crypto.Macs.HMac(digest);
                    crypto.Init(new Org.BouncyCastle.Crypto.Parameters.KeyParameter(keyData));
                    signature = signatureBase.HashWith(crypto);
#else
                    using (var digest = new System.Security.Cryptography.HMACSHA1(keyData))
                        signature = signatureBase.HashWith(digest);
#endif
                    break;
                }
                case OAuthSignatureMethod.PlainText:
                {
                    signature = key;
                    break;
                }
                default:
                    throw new NotImplementedException("Only HMAC-SHA1 is currently supported.");
            }
            var result = signatureTreatment == OAuthSignatureTreatment.Escaped
                ? UrlEncodeRelaxed(signature)
                : signature;
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace RestSharp.Portable
{
    /// <summary>
    /// Utilities for the authorization header management
    /// </summary>
    public static class AuthHeaderUtilities
    {
        private static readonly IEnumerable<string> _emptyHeaderValues = new List<string>();

        /// <summary>
        /// Get the absolute request URL
        /// </summary>
        /// <param name="client">HTTP client</param>
        /// <param name="request">HTTP request message</param>
        /// <returns>The absolute URI</returns>
        [NotNull]
        public static Uri GetRequestUri([CanBeNull] this IHttpClient client, [NotNull] IHttpRequestMessage request)
        {
            if (client?.BaseAddress == null)
                return request.RequestUri;

            if (request.RequestUri != null)
                return new Uri(client.BaseAddress, request.RequestUri);

            return client.BaseAddress;
        }

        /// <summary>
        /// Get the absolute response or request URL
        /// </summary>
        /// <param name="client">HTTP client</param>
        /// <param name="request">HTTP request message</param>
        /// <param name="response">HTTP response message</param>
        /// <returns>The absolute URI</returns>
        [NotNull]
        public static Uri GetRequestUri([CanBeNull] this IHttpClient client, [NotNull] IHttpRequestMessage request, [NotNull] IHttpResponseMessage response)
        {
            var requestUri = client.GetRequestUri(request);
            IEnumerable<string> locationValues;
            if (!response.Headers.TryGetValues("Location", out locationValues))
                return requestUri;
            var location = locationValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(location))
                return requestUri;
            return new Uri(requestUri, location);
        }

        /// <summary>
        /// Returns the HTTP header name for a given authorization header
        /// </summary>
        /// <param name="header">The authorization/authentication header</param>
        /// <returns>The HTTP header name</returns>
        [NotNull]
        public static string ToAuthorizationHeaderName(this AuthHeader header)
        {
            if (header == AuthHeader.Www)
                return "Authorization";
            return "Proxy-Authorization";
        }

        /// <summary>
        /// Returns the HTTP header name for a given authentication header
        /// </summary>
        /// <param name="header">The authorization/authentication header</param>
        /// <returns>The HTTP header name</returns>
        [NotNull]
        public static string ToAuthenticationHeaderName(this AuthHeader header)
        {
            if (header == AuthHeader.Www)
                return "WWW-Authenticate";
            return "Proxy-Authenticate";
        }

        /// <summary>
        /// Remove the authorization HTTP header if it's not equal to the old one.
        /// </summary>
        /// <param name="parameters">List of HTTP headers</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authorization header value</param>
        /// <returns>true = header removed, false = same header already exists, null = header not found</returns>
        public static bool? RemoveAuthorizationHeader([NotNull, ItemNotNull] IList<Parameter> parameters, AuthHeader header, [NotNull] string authValue)
        {
            var authParam = parameters.SingleOrDefault(
                p => p.Name.Equals(header.ToAuthorizationHeaderName(), StringComparison.OrdinalIgnoreCase));
            if (authParam == null)
                return null;
            var v = (string)authParam.Value;
            if (v != null && v == authValue)
                return false;
            parameters.Remove(authParam);
            return true;
        }

        /// <summary>
        /// Remove the authorization header from both the client and the request
        /// </summary>
        /// <param name="client">The client to be searched for the HTTP authorization header</param>
        /// <param name="request">The request to be searched for the HTTP authorization header</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authorization header value</param>
        /// <returns>true when the authorization header can be added again</returns>
        public static bool RemoveAuthorizationHeader([NotNull] IRestClient client, [NotNull] IRestRequest request, AuthHeader header, [NotNull] string authValue)
        {
            var result = RemoveAuthorizationHeader(client.DefaultParameters, header, authValue);
            if (result.HasValue && !result.Value)
                return false;
            return RemoveAuthorizationHeader(request.Parameters, header, authValue).GetValueOrDefault(true);
        }

        /// <summary>
        /// Unconditionally adds the authorization header to the request
        /// </summary>
        /// <param name="request">The request to add the authorization header to</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authorization header value</param>
        public static void SetAuthorizationHeader([NotNull] IRestRequest request, AuthHeader header, [NotNull] string authValue)
        {
            request.AddParameter(header.ToAuthorizationHeaderName(), authValue, ParameterType.HttpHeader);
        }

        /// <summary>
        /// Unconditionally adds the authorization header to the request
        /// </summary>
        /// <param name="request">The request to add the authorization header to</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authentication header value</param>
        public static void SetAuthorizationHeader([NotNull] this IHttpRequestMessage request, AuthHeader header, [NotNull] string authValue)
        {
            var headerName = header.ToAuthorizationHeaderName();
            request.Headers.Remove(headerName);
            request.Headers.Add(headerName, authValue);
        }

        /// <summary>
        /// Try to set the authorization header
        /// </summary>
        /// <param name="client">The client to remove the old authorization header from</param>
        /// <param name="request">The request to remove the old authorization header from and to add the new header to</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authorization header value</param>
        /// <returns>true when the authorization header could be set</returns>
        public static bool TrySetAuthorizationHeader([NotNull] IRestClient client, [NotNull] IRestRequest request, AuthHeader header, [NotNull] string authValue)
        {
            if (!RemoveAuthorizationHeader(client, request, header, authValue))
                return false;
            SetAuthorizationHeader(request, header, authValue);
            return true;
        }

        /// <summary>
        /// Get all authentication header information
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <returns>All authentication header information items</returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<AuthHeaderInfo> GetAuthenticationHeaderInfo([NotNull] this IRestResponse response, AuthHeader header)
        {
            var headerName = header.ToAuthenticationHeaderName();
            return response
                .Headers.GetValues(headerName)
                .SelectMany(ParseAuthenticationHeader)
                .ToList();
        }

        /// <summary>
        /// Try to get the authentication header value for a given authentication method
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <param name="methodName">The method name to query the value for</param>
        /// <returns>The information attached to the authentication header for a given method</returns>
        [CanBeNull]
        public static string GetAuthenticationMethodValue([NotNull] this IRestResponse response, AuthHeader header, [NotNull] string methodName)
        {
            return GetAuthenticationHeaderInfo(response, header)
                .Where(x => string.Equals(x.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.RawValue)
                .SingleOrDefault();
        }

        /// <summary>
        /// Get all authentication header information
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <returns>All authentication header information items</returns>
        [NotNull, ItemNotNull]
        public static IEnumerable<AuthHeaderInfo> GetAuthenticationHeaderInfo([NotNull] this IHttpResponseMessage response, AuthHeader header)
        {
            var headerName = header.ToAuthenticationHeaderName();
            IEnumerable<string> headerValues;
            if (!response.Headers.TryGetValues(headerName, out headerValues))
                headerValues = _emptyHeaderValues;
            return headerValues
                .SelectMany(ParseAuthenticationHeader)
                .ToList();
        }

        /// <summary>
        /// Try to get the authentication header value for a given authentication method
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <param name="methodName">The method name to query the value for</param>
        /// <returns>The information attached to the authentication header for a given method</returns>
        [CanBeNull]
        public static string GetAuthenticationMethodValue([NotNull] this IHttpResponseMessage response, AuthHeader header, [NotNull] string methodName)
        {
            return GetAuthenticationHeaderInfo(response, header)
                .Where(x => string.Equals(x.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.RawValue)
                .SingleOrDefault();
        }

        /// <summary>
        /// Parse a string and extract all <see cref="AuthHeaderInfo"/> entries.
        /// </summary>
        /// <param name="headerValue">The string to parse</param>
        /// <returns></returns>
        public static IEnumerable<AuthHeaderInfo> ParseAuthenticationHeader(string headerValue)
        {
            var whiteSpace = new[] { ' ', '\t', '\r', '\n' };
            var interestingChars = new[] { ' ', '\t', '\r', '\n', '"', '=', ',', '\\' };
            var interestingCharsWithoutWhitespace = new[] { '"', '=', ',', '\\' };
            var result = new List<AuthHeaderInfo>();
            var authHeaderValues = new List<KeyValuePair<string, string>>();
            var rawAuthHeaderValues = new List<KeyValuePair<string, string>>();
            var keyBuffer = new StringBuilder();
            var valueBuffer = new StringBuilder();
            var methodBuffer = new StringBuilder();
            var rawTempBuffer = new StringBuilder();
            var rawValueBuffer = new StringBuilder();
            var rawAuthHeaderValue = new StringBuilder();
            var state = ParseState.Method;
            int? escapePos = null;
            int? quotePos = null;
            var startPos = 0;
            var pos = headerValue.IndexOfAny(interestingChars, startPos);
            while (pos != -1)
            {
                StringBuilder activeBuffer;
                switch (state)
                {
                    case ParseState.Key:
                        activeBuffer = keyBuffer;
                        break;
                    case ParseState.Method:
                        activeBuffer = methodBuffer;
                        break;
                    case ParseState.Value:
                        activeBuffer = valueBuffer;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var appendValue = headerValue.Substring(startPos, pos - startPos);
                if (state == ParseState.Key || state == ParseState.Value)
                    rawTempBuffer.Append(appendValue);
                if (activeBuffer.Length == 0)
                    appendValue = appendValue.TrimStart(whiteSpace);
                if (appendValue.Length != 0)
                {
                    activeBuffer.Append(appendValue);
                    if (state == ParseState.Value)
                        rawAuthHeaderValue.Append(appendValue);
                }

                var isQuoted = quotePos != null;
                var isEscaped = escapePos != null && escapePos == (pos - 1);
                if (!isEscaped && escapePos != null)
                    escapePos = null;

                startPos = pos + 1;

                var ch = headerValue[pos];
                switch (ch)
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        if (activeBuffer.Length != 0 || isQuoted || isEscaped)
                        {
                            if (isQuoted || isEscaped)
                            {
                                activeBuffer.Append(ch);
                                rawTempBuffer.Append(ch);
                                if (state == ParseState.Value)
                                    rawAuthHeaderValue.Append(ch);
                            }
                            else if (state == ParseState.Method)
                            {
                                state = ParseState.Key;
                            }
                            else if (state == ParseState.Key)
                            {
                                var testPos = headerValue.IndexOfAny(interestingCharsWithoutWhitespace, pos + 1);
                                var paramName =
                                    (testPos == -1
                                         ? headerValue.Substring(pos + 1)
                                         : headerValue.Substring(pos, testPos - pos - 1)).Trim(whiteSpace);
                                if (paramName.Length != 0)
                                {
                                    // Found new method
                                    var method = methodBuffer.ToString().Trim(whiteSpace);
                                    if (!string.IsNullOrEmpty(method))
                                    {
                                        var rawValue = rawValueBuffer.ToString().Trim(whiteSpace);
                                        if (rawValue.StartsWith(","))
                                            rawValue = rawValue.Substring(1).TrimStart(whiteSpace);
                                        result.Add(new AuthHeaderInfo(method, rawValue, authHeaderValues, rawAuthHeaderValues));
                                    }

                                    var newMethod = keyBuffer.ToString().Trim(whiteSpace);
                                    if (newMethod.StartsWith(","))
                                        newMethod = newMethod.Substring(1).TrimStart(whiteSpace);
                                    rawValueBuffer.Clear();
                                    authHeaderValues.Clear();
                                    rawAuthHeaderValues.Clear();
                                    methodBuffer.Clear();
                                    methodBuffer.Append(newMethod);
                                    keyBuffer.Clear();
                                    rawTempBuffer.Clear();
                                }
                                else
                                {
                                    activeBuffer.Append(ch);
                                    rawTempBuffer.Append(ch);
                                    rawAuthHeaderValue.Append(ch);
                                }
                            }
                            else
                            {
                                activeBuffer.Append(ch);
                                rawTempBuffer.Append(ch);
                                if (state == ParseState.Value)
                                    rawAuthHeaderValue.Append(ch);
                            }
                        }
                        else if (state == ParseState.Key || state == ParseState.Value)
                        {
                            rawTempBuffer.Append(ch);
                        }
                        break;
                    case '"':
                        rawTempBuffer.Append(ch);
                        if (state == ParseState.Value)
                            rawAuthHeaderValue.Append(ch);
                        if (isEscaped)
                        {
                            activeBuffer.Append(ch);
                        }
                        else if (!isQuoted)
                        {
                            quotePos = pos;
                        }
                        else
                        {
                            quotePos = null;
                        }
                        break;
                    case '=':
                        if (isEscaped || isQuoted)
                        {
                            activeBuffer.Append(ch);
                            rawTempBuffer.Append(ch);
                            if (state == ParseState.Value)
                                rawAuthHeaderValue.Append(ch);
                        }
                        else
                        {
                            state = ParseState.Value;
                            rawValueBuffer.Append(rawTempBuffer).Append(ch);
                            rawTempBuffer.Clear();
                        }
                        break;
                    case ',':
                        if (isEscaped || isQuoted)
                        {
                            activeBuffer.Append(ch);
                            rawTempBuffer.Append(ch);
                            if (state == ParseState.Value)
                                rawAuthHeaderValue.Append(ch);
                        }
                        else
                        {
                            var key = keyBuffer.ToString().Trim(whiteSpace);
                            var value = valueBuffer.ToString().Trim(whiteSpace);
                            var rawAuthValue = rawAuthHeaderValue.ToString().Trim(whiteSpace);
                            if (key.Length != 0 || value.Length != 0)
                            {
                                authHeaderValues.Add(new KeyValuePair<string, string>(key.ToLowerInvariant(), value));
                                rawAuthHeaderValues.Add(new KeyValuePair<string, string>(key, rawAuthValue));
                            }
                            state = ParseState.Key;
                            rawValueBuffer.Append(rawTempBuffer);
                            keyBuffer.Clear();
                            valueBuffer.Clear();
                            rawTempBuffer.Clear().Append(ch);
                            rawAuthHeaderValue.Clear();
                        }
                        break;
                    case '\\':
                        if (isEscaped)
                        {
                            escapePos = null;
                            activeBuffer.Append(ch);
                            rawTempBuffer.Append(ch);
                            if (state == ParseState.Value)
                                rawAuthHeaderValue.Append(ch);
                        }
                        else
                        {
                            escapePos = pos;
                            rawTempBuffer.Append(ch);
                            if (state == ParseState.Value)
                                rawAuthHeaderValue.Append(ch);
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }

                pos = headerValue.IndexOfAny(interestingChars, startPos);
            }

            if (startPos < headerValue.Length)
            {
                pos = headerValue.Length;

                StringBuilder activeBuffer;
                switch (state)
                {
                    case ParseState.Key:
                        activeBuffer = keyBuffer;
                        break;
                    case ParseState.Method:
                        activeBuffer = methodBuffer;
                        break;
                    case ParseState.Value:
                        activeBuffer = valueBuffer;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                var appendValue = headerValue.Substring(startPos, pos - startPos);
                if (activeBuffer.Length == 0)
                    appendValue = appendValue.TrimStart(whiteSpace);
                if (appendValue.Length != 0)
                {
                    if (state == ParseState.Key || state == ParseState.Value)
                        rawTempBuffer.Append(appendValue);
                    activeBuffer.Append(appendValue);
                    if (state == ParseState.Value)
                        rawAuthHeaderValue.Append(appendValue);
                }
            }

            if (methodBuffer.Length != 0)
            {
                if (keyBuffer.Length != 0)
                {
                    var key = keyBuffer.ToString().Trim(whiteSpace);
                    var value = valueBuffer.ToString().Trim(whiteSpace);
                    var rawAuthValue = rawAuthHeaderValue.ToString().Trim(whiteSpace);
                    authHeaderValues.Add(new KeyValuePair<string, string>(key.ToLowerInvariant(), value));
                    rawAuthHeaderValues.Add(new KeyValuePair<string, string>(key, rawAuthValue));
                }

                rawValueBuffer.Append(rawTempBuffer);
                var method = methodBuffer.ToString().Trim(whiteSpace);
                var rawValue = rawValueBuffer.ToString().Trim(whiteSpace);
                if (rawValue.StartsWith(","))
                    rawValue = rawValue.Substring(1).TrimStart(whiteSpace);

                result.Add(new AuthHeaderInfo(method, rawValue, authHeaderValues, rawAuthHeaderValues));
            }

            return result;
        }

        private enum ParseState
        {
            Method,
            Key,
            Value,
        }
    }
}

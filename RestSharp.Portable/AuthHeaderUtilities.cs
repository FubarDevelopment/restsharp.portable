using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

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
        public static Uri GetRequestUri([NotNull] this HttpClient client, [NotNull] HttpRequestMessage request)
        {
            if (client.BaseAddress == null)
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
        public static Uri GetRequestUri([NotNull] this HttpClient client, [NotNull] HttpRequestMessage request, [NotNull] HttpResponseMessage response)
        {
            var requestUri = client.GetRequestUri(request);
            if (response.Headers.Location == null)
                return requestUri;
            return new Uri(requestUri, response.Headers.Location);
        }

        /// <summary>
        /// Returns the HTTP header name for a given authorization header
        /// </summary>
        /// <param name="header">The authorization/authentication header</param>
        /// <returns>The HTTP header name</returns>
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
        public static bool? RemoveAuthorizationHeader(IList<Parameter> parameters, AuthHeader header, string authValue)
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
        public static bool RemoveAuthorizationHeader(IRestClient client, IRestRequest request, AuthHeader header, string authValue)
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
        public static void SetAuthorizationHeader(IRestRequest request, AuthHeader header, string authValue)
        {
            request.AddParameter(header.ToAuthorizationHeaderName(), authValue, ParameterType.HttpHeader);
        }

        /// <summary>
        /// Unconditionally adds the authorization header to the request
        /// </summary>
        /// <param name="request">The request to add the authorization header to</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authentication header value</param>
        public static void SetAuthorizationHeader(this HttpRequestMessage request, AuthHeader header, AuthenticationHeaderValue authValue)
        {
            switch (header)
            {
                case AuthHeader.Www:
                    request.Headers.Authorization = authValue;
                    break;
                case AuthHeader.Proxy:
                    request.Headers.ProxyAuthorization = authValue;
                    break;
            }
        }

        /// <summary>
        /// Try to set the authorization header
        /// </summary>
        /// <param name="client">The client to remove the old authorization header from</param>
        /// <param name="request">The request to remove the old authorization header from and to add the new header to</param>
        /// <param name="header">The type of the HTTP header that stores the authorization information</param>
        /// <param name="authValue">The authorization header value</param>
        /// <returns>true when the authorization header could be set</returns>
        public static bool TrySetAuthorizationHeader(IRestClient client, IRestRequest request, AuthHeader header, string authValue)
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
        public static IEnumerable<AuthHeaderInfo> GetAuthenticationHeaderInfo(this IRestResponse response, AuthHeader header)
        {
            var headerName = header.ToAuthenticationHeaderName();
            return response
                .Headers.GetValues(headerName)
                .Select(x => new AuthHeaderInfo(x))
                .ToList();
        }

        /// <summary>
        /// Try to get the authentication header value for a given authentication method
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <param name="methodName">The method name to query the value for</param>
        /// <returns>The information attached to the authentication header for a given method</returns>
        public static string GetAuthenticationMethodValue(this IRestResponse response, AuthHeader header, string methodName)
        {
            return GetAuthenticationHeaderInfo(response, header)
                .Where(x => string.Equals(x.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Info)
                .SingleOrDefault();
        }

        /// <summary>
        /// Get all authentication header information
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <returns>All authentication header information items</returns>
        public static IEnumerable<AuthHeaderInfo> GetAuthenticationHeaderInfo(this HttpResponseMessage response, AuthHeader header)
        {
            var headerName = header.ToAuthenticationHeaderName();
            IEnumerable<string> headerValues;
            if (!response.Headers.TryGetValues(headerName, out headerValues))
                headerValues = _emptyHeaderValues;
            return headerValues
                .Select(x => new AuthHeaderInfo(x))
                .ToList();
        }

        /// <summary>
        /// Try to get the authentication header value for a given authentication method
        /// </summary>
        /// <param name="response">The response to get the authentication header from</param>
        /// <param name="header">The header to query (WWW or proxy)</param>
        /// <param name="methodName">The method name to query the value for</param>
        /// <returns>The information attached to the authentication header for a given method</returns>
        public static string GetAuthenticationMethodValue(this HttpResponseMessage response, AuthHeader header, string methodName)
        {
            return GetAuthenticationHeaderInfo(response, header)
                .Where(x => string.Equals(x.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Info)
                .SingleOrDefault();
        }
    }
}

using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// HTTP Digest authenticator
    /// </summary>
    public class HttpDigestAuthenticator : IRoundTripAuthenticator
    {

        private readonly ICredentials _credentials;
        private string _realm;
        private string _nonce;
        private string _qop;
        private string _cnonce;
        private string _opaque;
        private DateTime _cnonceDate;
        private int _nc;

        /// <summary>
        /// Initializes the HTTP Digest authenticator with the given credentials
        /// </summary>
        /// <param name="credentials"></param>
        public HttpDigestAuthenticator(ICredentials credentials)
        {
            _credentials = credentials;
        }


        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        public void Authenticate(IRestClient client, IRestRequest request)
        {
            if (HasDigestHeader)
            {
                var digestHeader = GetDigestHeader(client, request);
                request.AddParameter("Authorization", digestHeader, ParameterType.HttpHeader);
            }
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        public void AuthenticationFailed(IRestClient client, IRestRequest request, IRestResponse response)
        {
            ParseResponseHeader(response);
        }

        private string CalculateMd5Hash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hash = new MD5Managed().ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private string GrabHeaderVar(string varName, string header, string defaultValue = null)
        {
            var regHeader = new Regex(string.Format(@"{0}=""([^""]*)""", varName));
            var matchHeader = regHeader.Match(header);
            if (matchHeader.Success)
                return matchHeader.Groups[1].Value;
            if (defaultValue == null)
                throw new System.Net.WebException(string.Format("Header {0} not found", varName), WebExceptionStatus.UnknownError);
            return defaultValue;
        }

        private bool HasDigestHeader
        {
            get
            {
                return !string.IsNullOrEmpty(_cnonce) && DateTime.Now.Subtract(_cnonceDate).TotalHours < 1.0;
            }
        }

        private string GetDigestHeader(IRestClient client, IRestRequest restRequest)
        {
            _nc = _nc + 1;

            var uri = client.BuildUrl(restRequest);
            var credential = _credentials.GetCredential(uri, "Digest");

            var pathAndQuery = uri.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);

            var ha1 = CalculateMd5Hash(string.Format("{0}:{1}:{2}", credential.UserName, _realm, credential.Password));
            var ha2 = CalculateMd5Hash(string.Format("{0}:{1}", restRequest.Method.Method, pathAndQuery));
            var digestResponse = CalculateMd5Hash(string.Format("{0}:{1}:{2:00000000}:{3}:{4}:{5}", ha1, _nonce, _nc, _cnonce, _qop, ha2));

            var result = new StringBuilder();
            result
                .AppendFormat("Digest username=\"{0}\"", credential.UserName)
                .AppendFormat(",realm=\"{0}\"", _realm)
                .AppendFormat(",nonce=\"{0}\"", _nonce)
                .AppendFormat(",uri=\"{0}\"", pathAndQuery)
                .AppendFormat(",algorithm=\"{0}\"", "MD5")
                .AppendFormat(",cnonce=\"{0}\"", _cnonce)
                .AppendFormat(",nc={0:D08}", _nc)
                .AppendFormat(",qop=\"{0}\"", _qop);
            if (!string.IsNullOrEmpty(_opaque))
                result
                    .AppendFormat(",opaque=\"{0}\"", _opaque);
            result
                .AppendFormat(",response=\"{0}\"", digestResponse);
            return result.ToString();
        }

        private void ParseResponseHeader(IRestResponse response)
        {
            var wwwAuthenticateHeader = response.Headers.GetValues("WWW-Authenticate").First();
            _realm = GrabHeaderVar("realm", wwwAuthenticateHeader);
            _nonce = GrabHeaderVar("nonce", wwwAuthenticateHeader);
            _qop = GrabHeaderVar("qop", wwwAuthenticateHeader);

            _nc = 0;
            _opaque = GrabHeaderVar("opaque", wwwAuthenticateHeader, string.Empty);
            _cnonce = new Random().Next(123400, 9999999).ToString(CultureInfo.InvariantCulture);
            _cnonceDate = DateTime.Now;
        }
    }
}

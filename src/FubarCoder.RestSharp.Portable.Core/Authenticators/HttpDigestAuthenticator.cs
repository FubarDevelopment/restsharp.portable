using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#if NETSTANDARD1_1
using RestSharp.Portable.Crypto;
#else
using System.Security.Cryptography;
#endif

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// HTTP Digest authenticator
    /// </summary>
    /// <remarks>
    /// Code was taken from <code>http://www.ifjeffcandoit.com/2013/05/16/digest-authentication-with-restsharp/</code>
    /// </remarks>
    public class HttpDigestAuthenticator : IAuthenticator
    {
        /// <summary>
        /// The authentication method ID used in HTTP authentication challenge
        /// </summary>
        public const string AuthenticationMethod = "Digest";

        private readonly AuthHeader _authHeader;

        private NetworkCredential _authCredential;

        private string _realm;

        private string _nonce;

        private QualityOfProtection _qop = QualityOfProtection.Undefined;

        private string _cnonce;

        private string _opaque;

        private Algorithm _algorithm = Algorithm.Undefined;

        private DateTime _cnonceDate;

        private int _nc;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDigestAuthenticator" /> class.
        /// </summary>
        public HttpDigestAuthenticator()
            : this(AuthHeader.Www)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDigestAuthenticator" /> class.
        /// </summary>
        /// <param name="authHeader">Authentication/Authorization header type</param>
        public HttpDigestAuthenticator(AuthHeader authHeader)
        {
            _authHeader = authHeader;
        }

        [Flags]
        private enum QualityOfProtection
        {
            Undefined = 0,
            Auth = 1,
            AuthInt = 2,
        }

        private enum Algorithm
        {
            Undefined = 0,
            MD5,

            // ReSharper disable once InconsistentNaming
            MD5sess,
        }

        /// <summary>
        /// Gets a value indicating whether the authenticator already as an authorization token available for pre-authentication.
        /// </summary>
        protected bool HasAuthorizationToken => !string.IsNullOrEmpty(_cnonce) && DateTime.Now.Subtract(_cnonceDate).TotalHours < 1.0;

        /// <summary>
        /// Does the authentication module supports pre-authentication for the given <see cref="IRestRequest" />?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public bool CanPreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            return false;
        }

        /// <summary>
        /// Does the authentication module supports pre-authentication?
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <returns>true when the authentication module supports pre-authentication</returns>
        public bool CanPreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            return HasAuthorizationToken;
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public Task PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <returns>The task the authentication is performed on</returns>
        public async Task PreAuthenticate(IHttpClient client, IHttpRequestMessage request, ICredentials credentials)
        {
            if (!CanPreAuthenticate(client, request, credentials))
            {
                throw new InvalidOperationException();
            }

            var digestHeader = await GetDigestHeader(client, request, _authCredential);
            var authHeaderValue = $"{AuthenticationMethod} {digestHeader}";
            request.SetAuthorizationHeader(_authHeader, authHeaderValue);
        }

        /// <summary>
        /// Determines if the authentication module can handle the challenge sent with the response.
        /// </summary>
        /// <param name="client">The REST client the response is assigned to</param>
        /// <param name="request">The REST request the response is assigned to</param>
        /// <param name="credentials">The credentials to be used for the authentication</param>
        /// <param name="response">The response that returned the authentication challenge</param>
        /// <returns>true when the authenticator can handle the sent challenge</returns>
        public virtual bool CanHandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            // No credentials defined?
            if (credentials == null)
            {
                return false;
            }

            // No challenge header found?
            var authModeInfo = response.GetAuthenticationMethodValue(_authHeader, AuthenticationMethod);
            if (authModeInfo == null)
            {
                return false;
            }

            // Search for credential for request URI
            var responseUri = client.GetRequestUri(request, response);
            var credential = credentials.GetCredential(responseUri, AuthenticationMethod);
            if (credential == null)
            {
                return false;
            }

            // Did we already try to use the found credentials?
            if (ReferenceEquals(credential, _authCredential))
            {
                // Yes, so we don't retry the authentication.
                return false;
            }

            return true;
        }

        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="credentials">The credentials used for the authentication</param>
        /// <param name="response">Response of the failed request</param>
        /// <returns>Task where the handler for a failed authentication gets executed</returns>
        public Task HandleChallenge(IHttpClient client, IHttpRequestMessage request, ICredentials credentials, IHttpResponseMessage response)
        {
            return Task.Factory.StartNew(() =>
            {
                if (!CanHandleChallenge(client, request, credentials, response))
                {
                    throw new InvalidOperationException();
                }

                var responseUri = client.GetRequestUri(request, response);
                _authCredential = credentials.GetCredential(responseUri, AuthenticationMethod);
                var authModeInfo = response.GetAuthenticationMethodValue(_authHeader, AuthenticationMethod);
                ParseResponseHeader(authModeInfo);
            });
        }

        private static string CalculateMd5Hash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return CalculateMd5Hash(inputBytes);
        }

        private static string CalculateMd5Hash(byte[] inputBytes)
        {
            byte[] hash;
            using (var digest = MD5.Create())
            {
                hash = digest.ComputeHash(inputBytes);
            }

            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }

        private static string GrabHeaderVar(string varName, string header, string defaultValue = null)
        {
            var regHeader = new Regex($@"{varName}\s*=\s*((""(?<qval>[^""]*)"")|(?<val>[^,]+))");
            var matchHeader = regHeader.Match(header);
            if (matchHeader.Success)
            {
                var qval = matchHeader.Groups["qval"];
                if (qval.Success)
                {
                    return qval.Value;
                }

                var val = matchHeader.Groups["val"];
                return val.Value.Trim();
            }

            if (defaultValue == null)
            {
                throw new InvalidOperationException($"Header {varName} not found");
            }

            return defaultValue;
        }

        private async Task<string> GetDigestHeader(IHttpClient client, IHttpRequestMessage request, NetworkCredential credential)
        {
            _nc = _nc + 1;

            var uri = client.GetRequestUri(request);

            var pathAndQuery = uri.GetComponents(UriComponents.PathAndQuery, UriFormat.SafeUnescaped);

            if (_algorithm == Algorithm.Undefined)
            {
                throw new InvalidOperationException("Algorithm not set");
            }

            string ha2, digestResponse;

            var ha1 = CalculateMd5Hash($"{credential.UserName}:{_realm}:{credential.Password}");

            string algorithm;
            switch (_algorithm)
            {
                case Algorithm.MD5sess:
                    ha1 = CalculateMd5Hash($"{ha1}:{_nonce}:{_cnonce}");
                    algorithm = "MD5-sess";
                    break;
                default:
                    algorithm = "MD5";
                    break;
            }

            string qop;
            switch (_qop)
            {
                case QualityOfProtection.Auth:
                    qop = "auth";
                    break;
                case QualityOfProtection.AuthInt:
                    qop = "auth-int";
                    break;
                default:
                    qop = null;
                    break;
            }

            switch (_qop)
            {
                case QualityOfProtection.AuthInt:
                    {
                        byte[] entityBody;
                        if (request.Content == null)
                        {
                            entityBody = new byte[0];
                        }
                        else
                        {
                            await request.Content.LoadIntoBufferAsync();
                            entityBody = await request.Content.ReadAsByteArrayAsync();
                        }

                        ha2 = CalculateMd5Hash(entityBody);
                    }

                    ha2 = CalculateMd5Hash($"{request.Method}:{pathAndQuery}:{ha2}");
                    break;
                default:
                    ha2 = CalculateMd5Hash($"{request.Method}:{pathAndQuery}");
                    break;
            }

            switch (_qop)
            {
                case QualityOfProtection.AuthInt:
                case QualityOfProtection.Auth:
                    digestResponse = CalculateMd5Hash($"{ha1}:{_nonce}:{_nc:D8}:{_cnonce}:{qop}:{ha2}");
                    break;
                default:
                    digestResponse = CalculateMd5Hash($"{ha1}:{_nonce}:{ha2}");
                    break;
            }

            var result = new StringBuilder();
            result
                .AppendFormat("username=\"{0}\"", credential.UserName)
                .AppendFormat(", realm=\"{0}\"", _realm)
                .AppendFormat(", nonce=\"{0}\"", _nonce)
                .AppendFormat(", uri=\"{0}\"", pathAndQuery)
                .AppendFormat(", nc={0:D08}", _nc);
            if (algorithm != "MD5")
            {
                result.AppendFormat(", algorithm=\"{0}\"", algorithm);
            }

            if (!string.IsNullOrEmpty(qop))
            {
                result
                    .AppendFormat(", cnonce=\"{0}\"", _cnonce)
                    .AppendFormat(", qop={0}", qop);
            }

            if (!string.IsNullOrEmpty(_opaque))
            {
                result
                    .AppendFormat(", opaque=\"{0}\"", _opaque);
            }

            result
                .AppendFormat(", response=\"{0}\"", digestResponse);

            return result.ToString();
        }

        private void ParseResponseHeader(string authenticateHeader)
        {
            _realm = GrabHeaderVar("realm", authenticateHeader);
            _nonce = GrabHeaderVar("nonce", authenticateHeader);

            var algorithm = GrabHeaderVar("algorithm", authenticateHeader, "MD5");
            switch (algorithm.ToLower())
            {
                case "md5":
                    _algorithm = Algorithm.MD5;
                    break;
                case "md5-sess":
                    _algorithm = Algorithm.MD5sess;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported algorithm {algorithm}");
            }

            var qopParts = GrabHeaderVar("qop", authenticateHeader, string.Empty)
                .Split(',');
            _qop = QualityOfProtection.Undefined;
            foreach (var qopPart in qopParts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToLower()))
            {
                switch (qopPart)
                {
                    case "auth":
                        _qop |= QualityOfProtection.Auth;
                        break;
                    case "auth-int":
                        _qop |= QualityOfProtection.AuthInt;
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported QOP {qopPart}");
                }
            }

            _nc = 0;
            _opaque = GrabHeaderVar("opaque", authenticateHeader, string.Empty);
            _cnonce = new Random().Next(123400, 9999999).ToString(CultureInfo.InvariantCulture);
            _cnonceDate = DateTime.Now;
        }
    }
}

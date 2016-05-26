using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2
{
    /// <summary>
    /// Base class for OAuth2 client implementation.
    /// </summary>
    public abstract class OAuth2Client : IClient
    {
        private const string _accessTokenKey = "access_token";

        private const string _refreshTokenKey = "refresh_token";

        private const string _expiresKey = "expires_in";

        private const string _tokenTypeKey = "token_type";

        private const string _grantTypeAuthorizationKey = "authorization_code";

        private const string _grantTypeRefreshTokenKey = "refresh_token";

        private readonly IRequestFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2Client"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        protected OAuth2Client(IRequestFactory factory, IClientConfiguration configuration)
        {
            ExpirationSafetyMargin = TimeSpan.FromSeconds(5);
            _factory = factory;
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the client configuration object.
        /// </summary>
        public IClientConfiguration Configuration { get; }

        /// <summary>
        /// Gets the friendly name of provider (OAuth2 service).
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the state (any additional information that was provided by application and is posted back by service).
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Gets or sets the access token returned by provider. Can be used for further calls of provider API.
        /// </summary>
        public string AccessToken { get; protected set; }

        /// <summary>
        /// Gets or sets the refresh token returned by provider. Can be used for further calls of provider API.
        /// </summary>
        public string RefreshToken { get; protected set; }

        /// <summary>
        /// Gets the token type returned by provider. Can be used for further calls of provider API.
        /// </summary>
        public string TokenType { get; private set; }

        /// <summary>
        /// Gets or sets the time when the access token expires
        /// </summary>
        public DateTime? ExpiresAt { get; protected set; }

        /// <summary>
        /// Gets or sets a safety margin that's used to see if an access token is expired
        /// </summary>
        public TimeSpan ExpirationSafetyMargin { get; set; }

        /// <summary>
        /// Gets the instance of the request factory.
        /// </summary>
        protected IRequestFactory Factory => _factory;

        /// <summary>
        /// Gets the URI of service which issues access code.
        /// </summary>
        protected abstract Endpoint AccessCodeServiceEndpoint { get; }

        /// <summary>
        /// Gets the URI of service which issues access token.
        /// </summary>
        protected abstract Endpoint AccessTokenServiceEndpoint { get; }

        /// <summary>
        /// Gets the URI of service which allows to obtain information about user
        /// who is currently logged in.
        /// </summary>
        protected abstract Endpoint UserInfoServiceEndpoint { get; }

        private string GrantType { get; set; }

        /// <summary>
        /// Returns URI of service which should be called in order to start authentication process.
        /// This URI should be used for rendering login link.
        /// </summary>
        /// <param name="state">Any additional information that will be posted back by service.</param>
        /// <returns>A string containing the login link URI</returns>
        public virtual async Task<string> GetLoginLinkUri(string state = null)
        {
            var client = _factory.CreateClient(AccessCodeServiceEndpoint);
            var request = _factory.CreateRequest(AccessCodeServiceEndpoint);
            request.AddObject(
                new
                    {
                        response_type = "code",
                        client_id = Configuration.ClientId,
                        redirect_uri = Configuration.RedirectUri,
                        scope = Configuration.Scope,
                        state
                    },
                new[] { (string.IsNullOrEmpty(Configuration.Scope) ? "scope" : null) },
                PropertyFilterMode.Exclude);
            await BeforeGetLoginLinkUri(
                new BeforeAfterRequestArgs()
                    {
                        Client = client,
                        Request = request,
                        Configuration = Configuration,
                    });
            return client.BuildUri(request).ToString();
        }

        /// <summary>
        /// Obtains user information using RestSharp.Portable.OAuth2 service and data provided via callback request.
        /// </summary>
        /// <param name="parameters">Callback request payload (parameters).</param>
        /// <returns>The user information</returns>
        public async Task<UserInfo> GetUserInfo(ILookup<string, string> parameters)
        {
            GrantType = _grantTypeAuthorizationKey;
            CheckErrorAndSetState(parameters);
            await QueryAccessToken(parameters);
            return await GetUserInfo();
        }

        /// <summary>
        /// Issues query for access token and returns access token.
        /// </summary>
        /// <param name="parameters">Callback request payload (parameters).</param>
        /// <returns>The access token</returns>
        public async Task<string> GetToken(ILookup<string, string> parameters)
        {
            GrantType = _grantTypeAuthorizationKey;
            CheckErrorAndSetState(parameters);
            await QueryAccessToken(parameters);
            return AccessToken;
        }

        /// <summary>
        /// Get the current access token - and optionally refreshes it if it is expired
        /// </summary>
        /// <param name="refreshToken">The refresh token to use (null == default)</param>
        /// <param name="forceUpdate">Enforce an update of the access token?</param>
        /// <param name="safetyMargin">A custom safety margin to check if the access token is expired</param>
        /// <returns>The current access token</returns>
        public async Task<string> GetCurrentToken(string refreshToken = null, bool forceUpdate = false, TimeSpan? safetyMargin = null)
        {
            bool refreshRequired =
                forceUpdate
                || (ExpiresAt != null && DateTime.Now >= (ExpiresAt - (safetyMargin ?? ExpirationSafetyMargin)))
                || string.IsNullOrEmpty(AccessToken);

            if (refreshRequired)
            {
                string refreshTokenValue;
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    RefreshToken = refreshToken;
                    refreshTokenValue = refreshToken;
                }
                else if (!string.IsNullOrEmpty(RefreshToken))
                    refreshTokenValue = RefreshToken;
                else
                    throw new Exception("Token never fetched and refresh token not provided.");

                var parameters = new Dictionary<string, string>()
                    {
                        { _refreshTokenKey, refreshTokenValue },
                    };

                GrantType = _grantTypeRefreshTokenKey;
                await QueryAccessToken(parameters.ToLookup(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase));
            }

            return AccessToken;
        }

        /// <summary>
        /// Parse the response, search for a key and return its value.
        /// </summary>
        /// <param name="content">The content to parse</param>
        /// <param name="key">The key to query</param>
        /// <returns>The value for the queried key</returns>
        /// <exception cref="UnexpectedResponseException">Thrown when the key wasn't found</exception>
        protected static string ParseStringResponse(string content, string key)
        {
            var values = ParseStringResponse(content, new[] { key })[key].ToList();
            if (values.Count == 0)
                throw new UnexpectedResponseException(key);
            return values.First();
        }

        /// <summary>
        /// Parse the response for a given key/value using either JSON or form url encoded parameters
        /// </summary>
        /// <param name="content">The content to parse</param>
        /// <param name="keys">The keys to query</param>
        /// <returns>The values for the queried keys</returns>
        protected static ILookup<string, string> ParseStringResponse(string content, params string[] keys)
        {
            var result = new List<KeyValuePair<string, string>>();
            try
            {
                // response can be sent in JSON format
                var jobj = JObject.Parse(content);
                foreach (var key in keys)
                {
                    foreach (var token in jobj.SelectTokens(key))
                        if (token.HasValues)
                        {
                            foreach (var value in token.Values())
                                result.Add(new KeyValuePair<string, string>(key, (string)value));
                        }
                        else
                            result.Add(new KeyValuePair<string, string>(key, (string)token));
                }
            }
            catch (JsonReaderException)
            {
                // or it can be in "query string" format (param1=val1&param2=val2)
                var collection = content.ParseQueryString();
                foreach (var key in keys)
                {
                    foreach (var item in collection[key])
                        result.Add(new KeyValuePair<string, string>(key, item));
                }
            }

            return result.ToLookup(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parse the access token response using either JSON or form url encoded parameters
        /// </summary>
        /// <param name="content">The content to parse the access token from</param>
        /// <returns>The access token</returns>
        protected virtual string ParseAccessTokenResponse(string content)
        {
            return ParseStringResponse(content, _accessTokenKey);
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> using content received from provider.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        /// <returns>The found user information</returns>
        protected abstract UserInfo ParseUserInfo(IRestResponse response);

        /// <summary>
        /// Called just before building the request URI when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        /// <param name="args">The request/response arguments</param>
        /// <returns>The task this handler is processed on</returns>
        protected virtual Task BeforeGetLoginLinkUri(BeforeAfterRequestArgs args)
        {
#if USE_TASKEX
            return TaskEx.FromResult(0);
#else
            return Task.FromResult(0);
#endif
        }

        /// <summary>
        /// Called before the request to get the access token
        /// </summary>
        /// <param name="args">The request/response arguments</param>
        protected virtual void BeforeGetAccessToken(BeforeAfterRequestArgs args)
        {
            args.Request.AddObject(
                new
                {
                    client_id = Configuration.ClientId,
                    client_secret = Configuration.ClientSecret,
                    grant_type = GrantType
                });
            if (GrantType == _grantTypeRefreshTokenKey)
            {
                args.Request.AddObject(
                    new
                    {
                        refresh_token = args.Parameters.GetOrThrowUnexpectedResponse(_refreshTokenKey),
                    });
            }
            else
            {
                args.Request.AddObject(
                    new
                    {
                        code = args.Parameters.GetOrThrowUnexpectedResponse("code"),
                        redirect_uri = Configuration.RedirectUri,
                    });
            }
        }

        /// <summary>
        /// Called just after obtaining response with access token from service.
        /// Allows to read extra data returned along with access token.
        /// </summary>
        /// <param name="args">The request/response arguments</param>
        protected virtual void AfterGetAccessToken(BeforeAfterRequestArgs args)
        {
        }

        /// <summary>
        /// Called just before issuing request to service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        /// <param name="args">The request/response arguments</param>
        protected virtual void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
        }

        /// <summary>
        /// Obtains user information using provider API.
        /// </summary>
        /// <returns>The queried user information</returns>
        protected virtual async Task<UserInfo> GetUserInfo()
        {
            var client = _factory.CreateClient(UserInfoServiceEndpoint);
            client.Authenticator = new OAuth2UriQueryParameterAuthenticator(this);
            var request = _factory.CreateRequest(UserInfoServiceEndpoint);

            BeforeGetUserInfo(
                new BeforeAfterRequestArgs
                {
                    Client = client,
                    Request = request,
                    Configuration = Configuration
                });

            var response = await client.ExecuteAndVerify(request);

            var result = ParseUserInfo(response);
            result.ProviderName = Name;

            return result;
        }

        private void CheckErrorAndSetState(ILookup<string, string> parameters)
        {
            const string errorFieldName = "error";

            var error = parameters[errorFieldName].ToList();
            if (error.Any(x => !string.IsNullOrEmpty(x)))
                throw new UnexpectedResponseException(errorFieldName, string.Join("\n", error));

            State = string.Join(",", parameters["state"]);
        }

        /// <summary>
        /// Issues query for access token and parses response.
        /// </summary>
        /// <param name="parameters">Callback request payload (parameters).</param>
        /// <returns>The task the query is performed on</returns>
        private async Task QueryAccessToken(ILookup<string, string> parameters)
        {
            var client = _factory.CreateClient(AccessTokenServiceEndpoint);
            var request = _factory.CreateRequest(AccessTokenServiceEndpoint, Method.POST);

            BeforeGetAccessToken(
                new BeforeAfterRequestArgs
                    {
                        Client = client,
                        Request = request,
                        Parameters = parameters,
                        Configuration = Configuration
                    });

            var response = await client.ExecuteAndVerify(request);

            var content = response.Content;
            AccessToken = ParseAccessTokenResponse(content);

            if (GrantType != _grantTypeRefreshTokenKey)
                RefreshToken = ParseStringResponse(content, new[] { _refreshTokenKey })[_refreshTokenKey].FirstOrDefault();
            TokenType = ParseStringResponse(content, new[] { _tokenTypeKey })[_tokenTypeKey].FirstOrDefault();

            var expiresIn = ParseStringResponse(content, new[] { _expiresKey })[_expiresKey].Select(x => Convert.ToInt32(x, 10)).FirstOrDefault();
            ExpiresAt = expiresIn != 0 ? (DateTime?)DateTime.Now.AddSeconds(expiresIn) : null;

            AfterGetAccessToken(
                new BeforeAfterRequestArgs
                    {
                        Response = response,
                        Parameters = parameters
                    });
        }
    }
}

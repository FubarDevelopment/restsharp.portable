using System;
using System.Linq;
using System.Threading.Tasks;

using RestSharp.Portable.OAuth1;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2
{
    /// <summary>
    /// Base class for OAuth (version 1) client implementation.
    /// </summary>
    public abstract class OAuthClient : IClient
    {
        private const string _oAuthTokenKey = "oauth_token";

        private const string _oAuthTokenSecretKey = "oauth_token_secret";

        private readonly IRequestFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthClient" /> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        protected OAuthClient(IRequestFactory factory, IClientConfiguration configuration)
        {
            _factory = factory;
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the client configuration object.
        /// </summary>
        public IClientConfiguration Configuration { get; }

        /// <summary>
        /// Gets the friendly name of provider (OAuth service).
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the state which was posted as additional parameter
        /// to service and then received along with main answer.
        /// </summary>
        public string State => null;

        /// <summary>
        /// Gets the access token received from service. Can be used for further service API calls.
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the access token secret received from service. Can be used for further service API calls.
        /// </summary>
        public string AccessTokenSecret { get; private set; }

        /// <summary>
        /// Gets the URI of service which is called for obtaining request token.
        /// </summary>
        protected abstract Endpoint RequestTokenServiceEndpoint { get; }

        /// <summary>
        /// Gets the URI of service which should be called to initiate authentication process.
        /// </summary>
        protected abstract Endpoint LoginServiceEndpoint { get; }

        /// <summary>
        /// Gets the URI of service which issues access token.
        /// </summary>
        protected abstract Endpoint AccessTokenServiceEndpoint { get; }

        /// <summary>
        /// Gets the URI of service which is called to obtain user information.
        /// </summary>
        protected abstract Endpoint UserInfoServiceEndpoint { get; }

        /// <summary>
        /// Returns URI of service which should be called in order to start authentication process.
        /// You should use this URI when rendering login link.
        /// </summary>
        /// <param name="state">Any additional information needed by application.</param>
        /// <returns>Login link URI.</returns>
        public async Task<string> GetLoginLinkUri(string state = null)
        {
            if (!state.IsEmpty())
            {
                throw new NotSupportedException("State transmission is not supported by current implementation.");
            }

            await QueryRequestToken().ConfigureAwait(false);
            return GetLoginRequestUri(state);
        }

        /// <summary>
        /// Obtains user information using third-party authentication service
        /// using data provided via callback request.
        /// </summary>
        /// <param name="parameters">Callback request payload (parameters).
        /// <example>Request.QueryString</example></param>
        /// <returns>The user information</returns>
        public async Task<UserInfo> GetUserInfo(ILookup<string, string> parameters)
        {
            AccessToken = parameters.GetOrThrowUnexpectedResponse(_oAuthTokenKey);
            await QueryAccessToken(parameters.GetOrThrowUnexpectedResponse("oauth_verifier")).ConfigureAwait(false);

            var result = ParseUserInfo(await QueryUserInfo().ConfigureAwait(false));
            if (result != null)
                result.ProviderName = Name;

            return result;
        }

        /// <summary>
        /// Called just before obtaining response with access token from service.
        /// Allows to modify the request.
        /// </summary>
        /// <param name="args">The request arguments (client, request, configuration)</param>
        protected virtual void BeforeGetAccessToken(BeforeAfterRequestArgs args)
        {
        }

        /// <summary>
        /// Called just after obtaining response with access token from service.
        /// Allows to read extra data returned along with access token.
        /// </summary>
        /// <param name="args">The request arguments (response only)</param>
        protected virtual void AfterGetAccessToken(BeforeAfterRequestArgs args)
        {
        }

        /// <summary>
        /// Called just before issuing request to service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        /// <param name="args">The request arguments (client, request, configuration)</param>
        protected virtual void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> using content of callback issued by service.
        /// </summary>
        /// <param name="content">The content to parse the user information from</param>
        /// <returns>The new user information</returns>
        protected abstract UserInfo ParseUserInfo(string content);

        /// <summary>
        /// Issues request for request token and returns result.
        /// </summary>
        /// <returns>The task the request token gets queried on</returns>
        private async Task QueryRequestToken()
        {
            var client = _factory.CreateClient(RequestTokenServiceEndpoint);
            client.Authenticator = OAuth1Authenticator.ForRequestToken(
                Configuration.ClientId,
                Configuration.ClientSecret,
                Configuration.RedirectUri);

            var request = _factory.CreateRequest(RequestTokenServiceEndpoint, Method.POST);

            BeforeGetAccessToken(
                new BeforeAfterRequestArgs
                    {
                        Client = client,
                        Request = request,
                        Configuration = Configuration
                    });

            var response = await client.ExecuteAndVerify(request).ConfigureAwait(false);

            AfterGetAccessToken(
                new BeforeAfterRequestArgs
                {
                    Response = response,
                });

            var collection = response.Content.ParseQueryString();

            AccessToken = collection.GetOrThrowUnexpectedResponse(_oAuthTokenKey);
            AccessTokenSecret = collection.GetOrThrowUnexpectedResponse(_oAuthTokenSecretKey);
        }

        /// <summary>
        /// Composes login link URI.
        /// </summary>
        /// <param name="state">Any additional information needed by application.</param>
        /// <returns>The login request URI</returns>
        private string GetLoginRequestUri(string state = null)
        {
            var client = _factory.CreateClient(LoginServiceEndpoint);
            var request = _factory.CreateRequest(LoginServiceEndpoint);

            request.AddOrUpdateParameter(_oAuthTokenKey, AccessToken);
            if (!state.IsEmpty())
            {
                request.AddOrUpdateParameter("state", state);
            }

            return client.BuildUri(request).ToString();
        }

        /// <summary>
        /// Obtains access token by calling corresponding service.
        /// </summary>
        /// <param name="verifier">Verifier posted with callback issued by provider.</param>
        /// <returns>Access token and other extra info.</returns>
        private async Task QueryAccessToken(string verifier)
        {
            var client = _factory.CreateClient(AccessTokenServiceEndpoint);
            client.Authenticator = OAuth1Authenticator.ForAccessToken(
                Configuration.ClientId,
                Configuration.ClientSecret,
                AccessToken,
                AccessTokenSecret,
                verifier);

            var request = _factory.CreateRequest(AccessTokenServiceEndpoint, Method.POST);

            var content = (await client.ExecuteAndVerify(request).ConfigureAwait(false)).Content;
            var collection = content.ParseQueryString();

            AccessToken = collection.GetOrThrowUnexpectedResponse(_oAuthTokenKey);
            AccessTokenSecret = collection.GetOrThrowUnexpectedResponse(_oAuthTokenSecretKey);
        }

        /// <summary>
        /// Queries user info using corresponding service and data received by access token request.
        /// </summary>
        /// <returns>The task that queries the user information and the string that was returned by the query</returns>
        private async Task<string> QueryUserInfo()
        {
            var client = _factory.CreateClient(UserInfoServiceEndpoint);
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(
                Configuration.ClientId,
                Configuration.ClientSecret,
                AccessToken,
                AccessTokenSecret);

            var request = _factory.CreateRequest(UserInfoServiceEndpoint);

            BeforeGetUserInfo(
                new BeforeAfterRequestArgs
                {
                    Client = client,
                    Request = request,
                    Configuration = Configuration
                });

            return (await client.ExecuteAndVerify(request).ConfigureAwait(false)).Content;
        }
    }
}

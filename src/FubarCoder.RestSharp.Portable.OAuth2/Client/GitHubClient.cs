using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// GitHub authentication client.
    /// </summary>
    public class GitHubClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public GitHubClient(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Gets the friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "GitHub"; }
        }

        /// <summary>
        /// Gets the URI of service which issues access code.
        /// </summary>
        protected override Endpoint AccessCodeServiceEndpoint
        {
            get { return new Endpoint { BaseUri = "https://github.com", Resource = "/login/oauth/authorize" }; }
        }

        /// <summary>
        /// Gets the URI of service which issues access token.
        /// </summary>
        protected override Endpoint AccessTokenServiceEndpoint
        {
            get { return new Endpoint { BaseUri = "https://github.com", Resource = "/login/oauth/access_token" }; }
        }

        /// <summary>
        /// Gets the URI of service which allows to obtain information about user
        /// who is currently logged in.
        /// </summary>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get { return new Endpoint { BaseUri = "https://api.github.com/", Resource = "/user" }; }
        }

        /// <summary>
        /// Gets the URI of service which allows to obtain information about user email.
        /// </summary>
        protected virtual Endpoint UserEmailServiceEndpoint
        {
            get { return new Endpoint { BaseUri = "https://api.github.com/", Resource = "/user/emails" }; }
        }

        /// <summary>
        /// Called before the request to get the access token
        /// </summary>
        /// <param name="args">The request/response arguments</param>
        protected override void BeforeGetAccessToken(BeforeAfterRequestArgs args)
        {
            args.Request.AddObject(
                new
                    {
                        code = args.Parameters.GetOrThrowUnexpectedResponse("code"),
                        client_id = args.Configuration.ClientId,
                        client_secret = args.Configuration.ClientSecret,
                        redirect_uri = args.Configuration.RedirectUri,
                        state = State,
                    });
        }

        /// <summary>
        /// Obtains user information using provider API.
        /// </summary>
        /// <returns>The queried user information</returns>
        protected override async Task<UserInfo> GetUserInfo()
        {
            var userInfo = await base.GetUserInfo();
            if (userInfo == null)
                return null;

            if (!string.IsNullOrEmpty(userInfo.Email))
                return userInfo;

            var client = Factory.CreateClient(UserEmailServiceEndpoint);
            client.Authenticator = new OAuth2UriQueryParameterAuthenticator(this);
            var request = Factory.CreateRequest(UserEmailServiceEndpoint);

            BeforeGetUserInfo(new BeforeAfterRequestArgs
            {
                Client = client,
                Request = request,
                Configuration = Configuration
            });

            var response = await client.ExecuteAndVerify(request);
            var userEmails = ParseEmailAddresses(response.Content);
            userInfo.Email = userEmails.First(u => u.IsPrimary).Email;
            return userInfo;
        }

        /// <summary>
        /// Parse the email addresses
        /// </summary>
        /// <param name="content">The JSON response to parse</param>
        /// <returns>The list of user email addresses</returns>
        protected virtual IEnumerable<UserEmails> ParseEmailAddresses(string content)
        {
            return JsonConvert.DeserializeObject<List<UserEmails>>(content);
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> using content received from provider.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        /// <returns>The found user information</returns>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            var cnt = JObject.Parse(response.Content);
            var names = cnt["name"].Value<string>().Split(' ').ToList();
            const string avatarUriTemplate = "{0}&s={1}";
            var avatarUri = cnt["avatar_url"].Value<string>();
            var result = new UserInfo
                {
                    Email = cnt["email"].SafeGet(x => x.Value<string>()),
                    ProviderName = Name,
                    Id = cnt["id"].Value<string>(),
                    FirstName = names.Count > 0 ? names.First() : cnt["login"].Value<string>(),
                    LastName = names.Count > 1 ? names.Last() : string.Empty,
                    AvatarUri =
                        {
                            Small = !string.IsNullOrWhiteSpace(avatarUri) ? string.Format(avatarUriTemplate, avatarUri, AvatarInfo.SmallSize) : string.Empty,
                            Normal = avatarUri,
                            Large = !string.IsNullOrWhiteSpace(avatarUri) ? string.Format(avatarUriTemplate, avatarUri, AvatarInfo.LargeSize) : string.Empty
                        }
                };
            return result;
        }

        /// <summary>
        /// Information about a user email.
        /// </summary>
        protected class UserEmails
        {
            /// <summary>
            /// Gets or sets the email address
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this is the primary email address?
            /// </summary>
            public bool IsPrimary { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this email address is verified?
            /// </summary>
            public bool IsVerified { get; set; }
        }
    }
}

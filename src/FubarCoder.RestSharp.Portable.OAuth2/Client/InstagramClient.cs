using System;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Instagram authentication client.
    /// </summary>
    public class InstagramClient : OAuth2Client
    {
        private string _accessTokenResponseContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstagramClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public InstagramClient(IRequestFactory factory, IClientConfiguration configuration) 
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Defines URI of service which issues access code.
        /// </summary>
        protected override Endpoint AccessCodeServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.instagram.com",
                    Resource = "/oauth/authorize"
                };
            }
        }

        /// <summary>
        /// Defines URI of service which issues access token.
        /// </summary>
        protected override Endpoint AccessTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.instagram.com",
                    Resource = "/oauth/access_token"
                };
            }
        }

        /// <summary>
        /// Defines URI of service which allows to obtain information about user which is currently logged in.
        /// </summary>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                return new Endpoint
                {
                    BaseUri = "https://api.instagram.com",
                    Resource = "/oauth/access_token"
                };
            }
        }

        /// <summary>
        /// Called just after obtaining response with access token from service.
        /// Allows to read extra data returned along with access token.
        /// </summary>
        protected override void AfterGetAccessToken(BeforeAfterRequestArgs args)
        {
            // Instagram returns userinfo on access_token request
            // Source document 
            // http://instagram.com/developer/authentication/
            _accessTokenResponseContent = args.Response.Content;
        }

        /// <summary>
        /// Obtains user information using provider API.
        /// </summary>
        protected override Task<UserInfo> GetUserInfo()
        {
            var result = ParseUserInfo(_accessTokenResponseContent);

#if USE_TASKEX
            return TaskEx.FromResult(result);
#else
            return Task.FromResult(result);
#endif
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="content">The response which is received from the provider.</param>
        protected virtual UserInfo ParseUserInfo(string content)
        {
            var info = JObject.Parse(content);
            var names = info["user"]["full_name"].Value<string>().Split(' ');
            var avatarUri = info["user"]["profile_picture"].Value<string>();
            return new UserInfo
            {
                Id = info["user"]["id"].Value<string>(),
                FirstName = names.Any() ? names.First() : info["user"]["username"].Value<string>(),
                LastName = names.Count() > 1 ? names.Last() : string.Empty,
                AvatarUri =
                    {
                        Small = null,
                        Normal = avatarUri,
                        Large = null
                    }
            };
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Instagram"; }
        }
    }
}

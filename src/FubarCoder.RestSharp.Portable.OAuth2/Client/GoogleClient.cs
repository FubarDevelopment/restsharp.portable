using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;
using RestSharp.Portable;
using System.Threading.Tasks;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Google authentication client.
    /// </summary>
    public class GoogleClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public GoogleClient(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Called just before building the request URI when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override async Task BeforeGetLoginLinkUri(BeforeAfterRequestArgs args)
        {
            await base.BeforeGetLoginLinkUri(args);
            // This allows us to get a refresh token
            args.Request.AddOrUpdateParameter("access_type", "offline");
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
                    BaseUri = "https://accounts.google.com",
                    Resource = "/o/oauth2/auth"
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
                    BaseUri = "https://accounts.google.com",
                    Resource = "/o/oauth2/token"
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
                    BaseUri = "https://www.googleapis.com",
                    Resource = "/oauth2/v1/userinfo"
                };
            }
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Google"; }
        }


        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            var info = JObject.Parse(response.Content);
            var avatarUri = info["picture"].SafeGet(x => x.Value<string>());
            const string avatarUriTemplate = "{0}?sz={1}";
            return new UserInfo
            {
                Id = info["id"].Value<string>(),
                Email = info["email"].SafeGet(x => x.Value<string>()),
                FirstName = info["given_name"].Value<string>(),
                LastName = info["family_name"].Value<string>(),
                AvatarUri =
                    {
                        Small = !string.IsNullOrWhiteSpace(avatarUri) ? string.Format(avatarUriTemplate, avatarUri, AvatarInfo.SmallSize) : string.Empty,
                        Normal = avatarUri,
                        Large = !string.IsNullOrWhiteSpace(avatarUri) ? string.Format(avatarUriTemplate, avatarUri, AvatarInfo.LargeSize): string.Empty
                    }
            };
        }
    }
}
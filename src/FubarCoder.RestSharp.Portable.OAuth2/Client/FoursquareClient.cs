using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;
using RestSharp.Portable;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Foursquare authentication client.
    /// </summary>
    public class FoursquareClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FoursquareClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public FoursquareClient(IRequestFactory factory, IClientConfiguration configuration) 
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
                    BaseUri = "https://foursquare.com",
                    Resource = "/oauth2/authorize"
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
                    BaseUri = "https://foursquare.com",
                    Resource = "/oauth2/access_token"
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
                    BaseUri = "https://api.foursquare.com",
                    Resource = "/v2/users/self"
                };
            }
        }

        /// <summary>
        /// Called just before issuing request to third-party service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
            // Source documents 
            // https://developer.foursquare.com/overview/auth.html
            // https://developer.foursquare.com/overview/versioning
            args.Request.AddOrUpdateParameter("v", System.DateTime.Now.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            var info = JObject.Parse(response.Content);
            var prefix = info["response"]["user"]["photo"]["prefix"].Value<string>();
            var suffix = info["response"]["user"]["photo"]["suffix"].Value<string>();
            const string avatarUriTemplate = "{0}{1}{2}";
            const string avatarSizeTemplate = "{0}x{0}";
            return new UserInfo
            {

                Id = info["response"]["user"]["id"].Value<string>(),
                FirstName = info["response"]["user"]["firstName"].Value<string>(),
                LastName = info["response"]["user"]["lastName"].Value<string>(),
                Email = info["response"]["user"]["contact"]["email"].SafeGet(x => x.Value<string>()),                
                AvatarUri =
                {
                    // Defined photo sizes https://developer.foursquare.com/docs/responses/photo
                    Small = !string.IsNullOrWhiteSpace(prefix) ? string.Format(avatarUriTemplate, prefix, string.Format(avatarSizeTemplate, AvatarInfo.SmallSize), suffix) : string.Empty,
                    Normal = !string.IsNullOrWhiteSpace(prefix) ? string.Format(avatarUriTemplate, prefix, string.Empty, suffix) : string.Empty,
                    Large = !string.IsNullOrWhiteSpace(prefix) ? string.Format(avatarUriTemplate, prefix, string.Format(avatarSizeTemplate, AvatarInfo.LargeSize), suffix) : string.Empty
                }
            };
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Foursquare"; }
        }
    }
}

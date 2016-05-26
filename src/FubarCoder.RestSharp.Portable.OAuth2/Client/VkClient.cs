using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;
using RestSharp.Portable;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// VK (Vkontakte) authentication client.
    /// </summary>
    public class VkClient : OAuth2Client
    {
        private string _userId;
    private string _email;

        /// <summary>
        /// Initializes a new instance of the <see cref="VkClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public VkClient(IRequestFactory factory, IClientConfiguration configuration)
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
                    BaseUri = "http://oauth.vk.com",
                    Resource = "/authorize"
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
                    BaseUri = "https://oauth.vk.com",
                    Resource = "/access_token"
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
                    BaseUri = "https://api.vk.com",
                    Resource = "/method/users.get"
                };
            }
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Vkontakte"; }
        }

        /// <summary>
        /// Called just after obtaining response with access token from third-party service.
        /// Allows to read extra data returned along with access token.
        /// </summary>
        protected override void AfterGetAccessToken(BeforeAfterRequestArgs args)
        {
            var instance = JObject.Parse(args.Response.Content);
            _userId = instance["user_id"].Value<string>();
            var email = instance["email"];
            if (email != null)
                _email = email.Value<string>();
        }

        /// <summary>
        /// Called just before issuing request to third-party service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
            args.Request.AddOrUpdateParameter("uids", _userId);
            args.Request.AddOrUpdateParameter("fields", "uid,first_name,last_name,photo");
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            var info = JObject.Parse(response.Content)["response"][0];
            var avatarUri = info["photo"].Value<string>();
            return new UserInfo
            {
                Email = _email,
                FirstName = info["first_name"].Value<string>(),
                LastName = info["last_name"].Value<string>(),
                Id = info["uid"].Value<string>(),
                AvatarUri =
                    {
                        Small = null,
                        Normal = avatarUri,
                        Large = null
                    }                
            };
        }
    }
}

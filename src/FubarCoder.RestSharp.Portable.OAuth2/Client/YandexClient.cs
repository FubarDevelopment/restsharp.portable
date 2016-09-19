using System.Linq;
using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;
using RestSharp.Portable;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Yandex authentication client.
    /// </summary>
    public class YandexClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YandexClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public YandexClient(IRequestFactory factory, IClientConfiguration configuration)
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
                    BaseUri = "https://oauth.yandex.ru",
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
                    BaseUri = "https://oauth.yandex.ru",
                    Resource = "/token"
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
                    BaseUri = "https://login.yandex.ru",
                    Resource = "/info"
                };
            }
        }

        /// <summary>
        /// Called just before issuing request to third-party service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
            // Source document 
            // http://api.yandex.com/oauth/doc/dg/yandex-oauth-dg.pdf
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            return ParseUserInfo(response.Content);
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="content">The response which is received from the provider.</param>
        protected virtual UserInfo ParseUserInfo(string content)
        {
            var info = JObject.Parse(content);
            var names = info["real_name"].Value<string>().Split(' ');
            return new UserInfo
            {
                Id = info["id"].Value<string>(),
                FirstName = names.Length != 0 ? names[0] : info["display_name"].Value<string>(),
                LastName = names.Length > 1 ? names[names.Length - 1] : string.Empty,
                Email = info["default_email"].SafeGet(x => x.Value<string>()),
            };
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Yandex"; }
        }
    }
}
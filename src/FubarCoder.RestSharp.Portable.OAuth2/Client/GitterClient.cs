using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Gitter authentication client.
    /// </summary>
    public class GitterClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitterClient"/> class.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        public GitterClient(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Gitter"; }
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
                    BaseUri = "https://gitter.im",
                    Resource = "/login/oauth/authorize"
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
                    BaseUri = "https://gitter.im",
                    Resource = "/login/oauth/token"
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
                        BaseUri = "https://api.gitter.im",
                        Resource = "/v1/user",
                    };
            }
        }

        /// <summary>
        /// Obtains user information using provider API.
        /// </summary>
        /// <returns>Returns generalized user information</returns>
        protected override async Task<UserInfo> GetUserInfo()
        {
            var client = Factory.CreateClient(UserInfoServiceEndpoint);
            client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(this);
            var request = Factory.CreateRequest(UserInfoServiceEndpoint);

            BeforeGetUserInfo(new BeforeAfterRequestArgs
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

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">
        /// The response which is received from third-party service.
        /// </param>
        /// <returns>
        /// Returns some standard user information.
        /// </returns>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            var data = JsonConvert.DeserializeObject<List<GitterUser>>(response.Content).Single();
            return new UserInfo
                {
                    Id = data.Id,
                    LastName = data.DisplayName,
                    AvatarUri =
                        {
                            Normal = data.AvatarUrlMedium,
                            Small = data.AvatarUrlSmall,
                        },
                };
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class GitterUser
        {
            public string Id { get; set; }

            // ReSharper disable once UnusedMember.Local
            public string Username { get; set; }

            public string DisplayName { get; set; }

            // ReSharper disable once UnusedMember.Local
            public string Url { get; set; }

            public string AvatarUrlSmall { get; set; }

            public string AvatarUrlMedium { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}

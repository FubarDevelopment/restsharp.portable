using System;
using System.Threading.Tasks;

using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

using Newtonsoft.Json;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// LinkedIn authentication client.
    /// </summary>
    public class LinkedInClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedInClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public LinkedInClient(IRequestFactory factory, IClientConfiguration configuration)
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
                    BaseUri  = "https://www.linkedin.com",
                    Resource = "/uas/oauth2/authorization"
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
                    BaseUri  = "https://www.linkedin.com",
                    Resource = "/uas/oauth2/accessToken"
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
                    BaseUri  = "https://api.linkedin.com",
                    Resource = "/v1/people/~:(id,email-address,first-name,last-name,picture-url)?format=json"
                };
            }
        }

        /// <summary>
        /// Returns URI of service which should be called in order to start authentication process.
        /// This URI should be used for rendering login link.
        /// </summary>
        /// <param name="state">
        /// Any additional information that will be posted back by service.
        /// </param>
        public override Task<string> GetLoginLinkUri(string state = null)
        {
            return base.GetLoginLinkUri(state ?? Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Called just before issuing request to service when everything is ready.
        /// Allows to add extra parameters to request or do any other needed preparations.
        /// </summary>
        protected override void BeforeGetUserInfo(BeforeAfterRequestArgs args)
        {
            args.Client.Authenticator = null;
            args.Request.Parameters.Add(new Parameter
            {
                Name  = "oauth2_access_token",
                Type  = ParameterType.GetOrPost,
                Value = AccessToken
            });
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
            var person = JsonConvert.DeserializeObject<Person>(content);

            var avatarUri = person.PictureUrl;
            var avatarSizeTemplate = "{0}_{0}";
            if (string.IsNullOrEmpty(avatarUri))
            {
                avatarUri = "https://www.linkedin.com/scds/common/u/images/themes/katy/ghosts/person/ghost_person_80x80_v1.png";
                avatarSizeTemplate = "{0}x{0}";
            }
            var avatarDefaultSize = string.Format(avatarSizeTemplate, 80);

            return new UserInfo
            {
                Id = person.Id,
                Email = person.Email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                AvatarUri =
                    {
                        Small  = avatarUri.Replace(avatarDefaultSize, string.Format(avatarSizeTemplate, AvatarInfo.SmallSize)),
                        Normal = avatarUri,
                        Large  = avatarUri.Replace(avatarDefaultSize, string.Format(avatarSizeTemplate, AvatarInfo.LargeSize))
                    }
            };
        }

        /// <summary>
        /// Friendly name of provider (OAuth service).
        /// </summary>
        public override string Name
        {
            get { return "LinkedIn"; }
        }

        private class Person
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("emailAddress")]
            public string Email { get; set; }

            [JsonProperty("firstName")]
            public string FirstName { get; set; }

            [JsonProperty("lastName")]
            public string LastName { get; set; }

            [JsonProperty("pictureUrl")]
            public string PictureUrl { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Xing authentication client.
    /// </summary>
    public class XingClient : OAuthClient
    {
        private const string _baseApiUrl = "https://api.xing.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="XingClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public XingClient(IRequestFactory factory, IClientConfiguration configuration)
            : base(factory, configuration)
        {
        }

        /// <summary>
        /// Friendly name of provider (OAuth service).
        /// </summary>
        public override string Name
        {
            get { return "Xing"; }
        }

        /// <summary>
        /// Defines URI of service which is called for obtaining request token.
        /// </summary>
        protected override Endpoint RequestTokenServiceEndpoint
        {
            get
            {
                return new Endpoint
                    {
                        BaseUri = _baseApiUrl,
                        Resource = "/v1/request_token"
                    };
            }
        }

        /// <summary>
        /// Defines URI of service which should be called to initiate authentication process.
        /// </summary>
        protected override Endpoint LoginServiceEndpoint
        {
            get
            {
                return new Endpoint
                    {
                        BaseUri = _baseApiUrl,
                        Resource = "/v1/authorize"
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
                        BaseUri = _baseApiUrl,
                        Resource = "/v1/access_token"
                    };
            }
        }

        /// <summary>
        /// Defines URI of service which is called to obtain user information.
        /// </summary>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                return new Endpoint
                    {
                        BaseUri = _baseApiUrl,
                        Resource = "/v1/users/me"
                    };
            }
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> using content of callback issued by service.
        /// </summary>
        protected override UserInfo ParseUserInfo(string content)
        {
            var users = JsonConvert.DeserializeObject<UserContainer>(content);
            var userInfo = new UserInfo();

            if (users != null && users.Users != null && users.Users.Count > 0)
            {
                userInfo.Id = users.Users[0].Id;
                userInfo.FirstName = users.Users[0].FirstName;
                userInfo.LastName = users.Users[0].LastName;
                userInfo.Email = users.Users[0].Email;
                if (users.Users[0].PhotoUrls != null)
                {
                    userInfo.AvatarUri.Small = users.Users[0].PhotoUrls.Small;
                    userInfo.AvatarUri.Normal = users.Users[0].PhotoUrls.Normal;
                    userInfo.AvatarUri.Large = users.Users[0].PhotoUrls.Large;
                }
            }

            return userInfo;
        }

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class UserContainer
        {
            [JsonProperty(PropertyName = "users")]
            public List<User> Users { get; set; }
        }

        private class User
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "first_name")]
            public string FirstName { get; set; }

            [JsonProperty(PropertyName = "last_name")]
            public string LastName { get; set; }

            [JsonProperty(PropertyName = "active_email")]
            public string Email { get; set; }

            [JsonProperty(PropertyName = "photo_urls")]
            public PhotoUrls PhotoUrls { get; set; }
        }

        private class PhotoUrls
        {
            [JsonProperty(PropertyName = "size_48x48")]
            public string Small { get; set; }

            [JsonProperty(PropertyName = "size_128x128")]
            public string Normal { get; set; }

            [JsonProperty(PropertyName = "size_256x256")]
            public string Large { get; set; }
        }

        // ReSharper restore ClassNeverInstantiated.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}

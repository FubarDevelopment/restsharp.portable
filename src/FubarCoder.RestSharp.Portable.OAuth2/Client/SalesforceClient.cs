using System;
using Newtonsoft.Json.Linq;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2.Client
{
    /// <summary>
    /// Salesforce authentication client.
    /// </summary>
    public class SalesforceClient : OAuth2Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceClient"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="configuration">The configuration.</param>
        public SalesforceClient(IRequestFactory factory, IClientConfiguration configuration)
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
                    BaseUri = "https://login.salesforce.com",
                    Resource = "/services/oauth2/authorize"
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
                    BaseUri = "https://login.salesforce.com",
                    Resource = "/services/oauth2/token"
                };
            }
        }

        /// <summary>
        /// URI for the Salesforce profile
        /// </summary>
        /// <remarks>
        /// Will be set after the retireval of the access token.
        /// </remarks>
        public string SalesforceProfileUrl { get; set; }

        /// <summary>
        /// Defines URI of service which allows to obtain information about user which is currently logged in.
        /// </summary>
        protected override Endpoint UserInfoServiceEndpoint
        {
            get
            {
                Uri uri = new Uri(SalesforceProfileUrl);
                return new Endpoint
                {
                    BaseUri = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
                    Resource = uri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped),
                };
            }
        }

        /// <summary>
        /// Friendly name of provider (OAuth2 service).
        /// </summary>
        public override string Name
        {
            get { return "Salesforce"; }
        }

        /// <summary>
        /// Parse the access token response using either JSON or form url encoded parameters
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected override string ParseAccessTokenResponse(string content)
        {
            // save the user's identity service url which is included in the response
            SalesforceProfileUrl = (string)JObject.Parse(content).SelectToken("id");
                
            return base.ParseAccessTokenResponse(content);
        }

        /// <summary>
        /// Should return parsed <see cref="UserInfo"/> from content received from third-party service.
        /// </summary>
        /// <param name="response">The response which is received from the provider.</param>
        protected override UserInfo ParseUserInfo(IRestResponse response)
        {
            var info = JObject.Parse(response.Content);

            return new UserInfo
            {
                Id = info["id"].Value<string>(),
                Email = info["email"].SafeGet(x => x.Value<string>()),
                FirstName = info["first_name"].Value<string>(),
                LastName = info["last_name"].Value<string>(),
                AvatarUri =
                    {
                        Small = info["photos"]["thumbnail"].Value<string>(),
                        Normal = info["photos"]["picture"].Value<string>(),
                        Large = null
                    }
            };
        }
    }
}
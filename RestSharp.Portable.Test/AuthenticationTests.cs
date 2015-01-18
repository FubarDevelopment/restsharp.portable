using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Authenticators.OAuth;
using Xunit;

namespace RestSharp.Portable.Test
{
    public class AuthenticationTests
    {
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class AuthenticationResult
        {
            public bool Authenticated { get; set; }
            public string User { get; set; }
        }

        [Fact]
        public async Task TestHttpBasicAuth()
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org")
            {
                CookieContainer = new CookieContainer(),
                Authenticator = new OptionalHttpBasicAuthenticator(username, password),
            })
            {
                var request = new RestRequest("basic-auth/{username}/{password}", HttpMethod.Get);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Fact]
        public async Task TestHttpBasicAuthHidden()
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org")
            {
                CookieContainer = new CookieContainer(),
                Authenticator = new OptionalHttpBasicAuthenticator(username, password),
            })
            {
                var request = new RestRequest("hidden-basic-auth/{username}/{password}", HttpMethod.Get);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Fact]
        public async Task TestHttpDigestAuth()
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org")
            {
                CookieContainer = new CookieContainer(),
                Authenticator =
                    new HttpDigestAuthenticator(new NetworkCredential(username, password))
            })
            {
                var request = new RestRequest("digest-auth/auth/{username}/{password}", HttpMethod.Get);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Fact]
        public async Task TestHttpDigestAuthInt()
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org/digest-auth")
            {
                CookieContainer = new CookieContainer(),
                Authenticator =
                    new HttpDigestAuthenticator(new NetworkCredential(username, password))
            })
            {
                var request = new RestRequest("auth-int/{username}/{password}", HttpMethod.Get);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Fact(Skip = "Requires authentication")]
        public async Task TestBitbucketOAuth10()
        {
            var secret = ConfigurationManager.AppSettings["bitbucket-api-secret"];
            var key = ConfigurationManager.AppSettings["bitbucket-api-key"];

            using (var client = new RestClient("https://bitbucket.org/api/")
            {
                CookieContainer = new CookieContainer()
            })
            {
                client.AddHandler("application/x-www-form-urlencoded", new DictionaryDeserializer());

                var auth = OAuth1Authenticator.ForRequestToken(key, secret,
                    "https://testapp.local/callback");
                auth.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;
                client.Authenticator = auth;

                //string token, token_secret;
                {
                    var request = new RestRequest("1.0/oauth/request_token", HttpMethod.Post);
                    var response = await client.Execute<IDictionary<string, string>>(request);

                    Assert.True(response.Data.ContainsKey("oauth_callback_confirmed"));
                    Assert.Equal("true", response.Data["oauth_callback_confirmed"]);

                    //token_secret = response.Data["oauth_token_secret"];
                    //token = response.Data["oauth_token"];
                }
            }
        }
    }
}

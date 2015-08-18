using System;
using System.Net;
using System.Threading.Tasks;

using RestSharp.Portable.Authenticators;
using RestSharp.Portable.HttpClient;
using RestSharp.Portable.HttpClient.Impl;
using RestSharp.Portable.Test.HttpBin;
using RestSharp.Portable.WebRequest.Impl;

using Xunit;

namespace RestSharp.Portable.Test
{
    [CLSCompliant(false)]
    public class AuthenticationTests : RestSharpTests
    {
        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestHttpBasicAuth(Type factoryType)
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org")
            {
                CookieContainer = new CookieContainer(),
                HttpClientFactory = CreateClientFactory(factoryType, false),
                Credentials = new NetworkCredential(username, password),
                Authenticator = new HttpBasicAuthenticator(),
            })
            {
                var request = new RestRequest("basic-auth/{username}/{password}", Method.GET);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestHttpBasicAuthHidden(Type factoryType)
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org")
            {
                CookieContainer = new CookieContainer(),
                HttpClientFactory = CreateClientFactory(factoryType, false),
                Credentials = new NetworkCredential(username, password),
                Authenticator = new HttpHiddenBasicAuthenticator(),
            })
            {
                var request = new RestRequest("hidden-basic-auth/{username}/{password}", Method.GET);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestHttpDigestAuth(Type factoryType)
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org")
            {
                CookieContainer = new CookieContainer(),
                HttpClientFactory = CreateClientFactory(factoryType, false),
                Credentials = new NetworkCredential(username, password),
                Authenticator = new HttpDigestAuthenticator()
            })
            {
                var request = new RestRequest("digest-auth/auth/{username}/{password}", Method.GET);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestHttpDigestAuthInt(Type factoryType)
        {
            const string username = "user name";
            const string password = "passwd";

            using (var client = new RestClient("http://httpbin.org/digest-auth")
            {
                CookieContainer = new CookieContainer(),
                HttpClientFactory = CreateClientFactory(factoryType, false),
                Credentials = new NetworkCredential(username, password),
                Authenticator = new HttpDigestAuthenticator()
            })
            {
                // httpbin.org only supports GET for digest-auth...
                var request = new RestRequest("auth-int/{username}/{password}", Method.GET);
                request.AddUrlSegment("username", username);
                request.AddUrlSegment("password", password);
                ////request.AddParameter("param1", "val1");
                var response = await client.Execute<AuthenticationResult>(request);

                Assert.True(response.Data.Authenticated);
                Assert.Equal(username, response.Data.User);

                ////Assert.NotNull(response.Data.Form);
                ////Assert.Equal(1, response.Data.Form.Count);
                ////Assert.Contains("param1", response.Data.Form.Keys);
                ////Assert.Equal("val1", response.Data.Form["param1"]);
            }
        }

#if FALSE
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

                var auth = OAuth1Authenticator.ForRequestToken(
                    key,
                    secret,
                    "https://testapp.local/callback");
                auth.ParameterHandling = OAuthParameterHandling.UrlOrPostParameters;
                client.Authenticator = auth;

                ////string token, token_secret;
                {
                    var request = new RestRequest("1.0/oauth/request_token", Method.POST);
                    var response = await client.Execute<IDictionary<string, string>>(request);

                    Assert.True(response.Data.ContainsKey("oauth_callback_confirmed"));
                    Assert.Equal("true", response.Data["oauth_callback_confirmed"]);

                    ////token_secret = response.Data["oauth_token_secret"];
                    ////token = response.Data["oauth_token"];
                }
            }
        }
#endif
    }
}

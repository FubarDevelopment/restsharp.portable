using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Net.Http;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class Authentication
    {
        class AuthenticationResult
        {
            public bool Authenticated { get; set; }
            public string User { get; set; }
        }

        [TestMethod]
        public async Task TestHttpBasicAuth()
        {
            const string username = "user name";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.OptionalHttpBasicAuthenticator(username, password);
            var request = new RestRequest("basic-auth/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.IsTrue(response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestHttpBasicAuthHidden()
        {
            const string username = "user name";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.OptionalHttpBasicAuthenticator(username, password);
            var request = new RestRequest("hidden-basic-auth/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.IsTrue(response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestHttpDigestAuth()
        {
            const string username = "user name";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.HttpDigestAuthenticator(new System.Net.NetworkCredential(username, password));
            var request = new RestRequest("digest-auth/auth/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.IsTrue(response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestHttpDigestAuthInt()
        {
            const string username = "user name";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.HttpDigestAuthenticator(new System.Net.NetworkCredential(username, password));
            var request = new RestRequest("digest-auth/auth-int/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.IsTrue(response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestBitbucketOAuth10()
        {
            var secret = System.Configuration.ConfigurationManager.AppSettings["bitbucket-api-secret"];
            var key = System.Configuration.ConfigurationManager.AppSettings["bitbucket-api-key"];

            var client = new RestClient("https://bitbucket.org/api/");
            client.CookieContainer = new System.Net.CookieContainer();
            client.AddHandler("application/x-www-form-urlencoded", new DictionaryDeserializer());

            var auth = Authenticators.OAuth1Authenticator.ForRequestToken(key, secret, "https://testapp.local/callback");
            auth.ParameterHandling = Authenticators.OAuth.OAuthParameterHandling.UrlOrPostParameters;
            client.Authenticator = auth;

            string token, token_secret;
            {
                var request = new RestRequest("1.0/oauth/request_token", HttpMethod.Post);
                var response = await client.Execute<IDictionary<string, string>>(request);

                Assert.IsTrue(response.Data.ContainsKey("oauth_callback_confirmed"));
                Assert.AreEqual("true", response.Data["oauth_callback_confirmed"]);

                token_secret = response.Data["oauth_token_secret"];
                token = response.Data["oauth_token"];
            }

        }
    }
}

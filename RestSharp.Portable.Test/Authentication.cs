using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

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
            const string username = "user";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.OptionalHttpBasicAuthenticator(username, password);
            var request = new RestRequest("basic-auth/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.AreEqual(true, response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestHttpBasicAuthHidden()
        {
            const string username = "user";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.OptionalHttpBasicAuthenticator(username, password);
            var request = new RestRequest("hidden-basic-auth/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.AreEqual(true, response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestHttpDigestAuth()
        {
            const string username = "user";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.HttpDigestAuthenticator(new System.Net.NetworkCredential(username, password));
            var request = new RestRequest("digest-auth/auth/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.AreEqual(true, response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }

        [TestMethod]
        public async Task TestHttpDigestAuthInt()
        {
            const string username = "user";
            const string password = "passwd";

            var client = new RestClient("http://httpbin.org");
            client.CookieContainer = new System.Net.CookieContainer();
            client.Authenticator = new Authenticators.HttpDigestAuthenticator(new System.Net.NetworkCredential(username, password));
            var request = new RestRequest("digest-auth/auth-int/{username}/{password}", System.Net.Http.HttpMethod.Get);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("password", password);
            var response = await client.Execute<AuthenticationResult>(request);

            Assert.AreEqual(true, response.Data.Authenticated);
            Assert.AreEqual(username, response.Data.User);
        }
    }
}

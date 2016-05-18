using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using RestSharp.Portable.HttpClient;

using Xunit;

namespace RestSharp.Portable.OAuth1.Tests
{
    public class AuthenticatorTests
    {
        private readonly IConfigurationRoot _configRoot;

        public AuthenticatorTests()
        {
            var cfgBuilder = new ConfigurationBuilder()
                .AddUserSecrets("restsharp.portable.oauth1.tests");
            _configRoot = cfgBuilder.Build();
        }

        [Fact]
        public async Task ProtectedResourceQueryAsPostComplexUtf8()
        {
            var auth = OAuth1Authenticator.ForProtectedResource("consumer-key", "consumer-secret", "access-token", "access-token-secret");
            auth.RandomNumberGenerator = new MyRandomNumberGenerator();
            auth.CreateTimestampFunc = () => ToUnixTime(new DateTime(2015, 11, 8, 11, 12, 13)).ToString();
            var client = new TestRestClient();
            var request = new RestRequest("test", Method.POST);
            request.AddParameter("status", "😈❤️😍🎉😜 😜👯🍻🎈🎤🎮🚀🌉✨");
            await auth.PreAuthenticate(client, request, null);
            var header = request.Parameters.FirstOrDefault(x => x.Name == "Authorization");
            Assert.NotNull(header);
            Assert.Equal("OAuth oauth_consumer_key=\"consumer-key\",oauth_nonce=\"abcdefghijklmnop\",oauth_signature=\"rXtn0AUYLME80k3dLcizx3wNLxk%3D\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1446981133\",oauth_token=\"access-token\",oauth_version=\"1.0\"", (string)header.Value);
        }

        [Fact]
        public async Task ProtectedResourceQueryComplexUtf8()
        {
            var auth = OAuth1Authenticator.ForProtectedResource("consumer-key", "consumer-secret", "access-token", "access-token-secret");
            auth.RandomNumberGenerator = new MyRandomNumberGenerator();
            auth.CreateTimestampFunc = () => ToUnixTime(new DateTime(2015, 11, 8, 11, 12, 13)).ToString();
            var client = new TestRestClient();
            var request = new RestRequest("test", Method.POST);
            request.AddParameter("status", "😈❤️😍🎉😜 😜👯🍻🎈🎤🎮🚀🌉✨", ParameterType.QueryString);
            await auth.PreAuthenticate(client, request, null);
            var header = request.Parameters.FirstOrDefault(x => x.Name == "Authorization");
            Assert.NotNull(header);
            Assert.Equal("OAuth oauth_consumer_key=\"consumer-key\",oauth_nonce=\"abcdefghijklmnop\",oauth_signature=\"rXtn0AUYLME80k3dLcizx3wNLxk%3D\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1446981133\",oauth_token=\"access-token\",oauth_version=\"1.0\"", (string)header.Value);
        }

        [Fact]
        public async Task ProtectedResourceQuerySpecialChars()
        {
            var auth = OAuth1Authenticator.ForProtectedResource("consumer-key", "consumer-secret", "access-token", "access-token-secret");
            auth.RandomNumberGenerator = new MyRandomNumberGenerator();
            auth.CreateTimestampFunc = () => ToUnixTime(new DateTime(2015, 11, 8, 11, 12, 13)).ToString();
            var client = new TestRestClient();
            var request = new RestRequest("test", Method.POST);
            request.AddParameter("status", ":/#&=", ParameterType.QueryString);
            await auth.PreAuthenticate(client, request, null);
            var header = request.Parameters.FirstOrDefault(x => x.Name == "Authorization");
            Assert.NotNull(header);
            Assert.Equal("OAuth oauth_consumer_key=\"consumer-key\",oauth_nonce=\"abcdefghijklmnop\",oauth_signature=\"1HnNNm%2BAJgKJPcNCuVINryGTPUI%3D\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1446981133\",oauth_token=\"access-token\",oauth_version=\"1.0\"", (string)header.Value);
        }

        [Fact]
        public async Task ProtectedResourceQuerySimpleUtf8()
        {
            var auth = OAuth1Authenticator.ForProtectedResource("consumer-key", "consumer-secret", "access-token", "access-token-secret");
            auth.RandomNumberGenerator = new MyRandomNumberGenerator();
            auth.CreateTimestampFunc = () => ToUnixTime(new DateTime(2015, 11, 8, 11, 12, 13)).ToString();
            var client = new TestRestClient();
            var request = new RestRequest("test", Method.POST);
            request.AddParameter("status", "☺", ParameterType.QueryString);
            await auth.PreAuthenticate(client, request, null);
            var header = request.Parameters.FirstOrDefault(x => x.Name == "Authorization");
            Assert.NotNull(header);
            Assert.Equal("OAuth oauth_consumer_key=\"consumer-key\",oauth_nonce=\"abcdefghijklmnop\",oauth_signature=\"SIDMGnDWsGNw8XKV9WrrdAgynSE%3D\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"1446981133\",oauth_token=\"access-token\",oauth_version=\"1.0\"", (string)header.Value);
        }

        [SkippableTheory]
        [InlineData(":/#&?")]
        [InlineData("-_.~'")]
        [InlineData("!*()")]
        [InlineData("=,;$@{}[]")]
        public async Task TestTumblr(string text)
        {
            var clientConfig = _configRoot.GetSection("Tumblr");

            // Set the configuration values from the command line with:
            //
            // dotnet user-secrets set Tumblr:ConsumerId my-consumer-id
            // dotnet user-secrets set Tumblr:ConsumerSecret my-consumer-secret
            // dotnet user-secrets set Tumblr:AccessToken my-access-token
            // dotnet user-secrets set Tumblr:AccessSecret my-access-secret
            // dotnet user-secrets set Tumblr:BlogHostname my-blog-hostname

            var consumerId = clientConfig["ConsumerId"];
            var consumerSecret = clientConfig["ConsumerSecret"];
            var accessToken = clientConfig["AccessToken"];
            var accessSecret = clientConfig["AccessSecret"];
            var hostName = clientConfig["BlogHostname"];

            var configEmpty = string.IsNullOrEmpty(consumerId)
                              || string.IsNullOrEmpty(consumerSecret)
                              || string.IsNullOrEmpty(accessToken)
                              || string.IsNullOrEmpty(accessSecret)
                              || string.IsNullOrEmpty(hostName);

            Skip.If(configEmpty, "Tumblr configuration empty");

            using (var client = new RestClient("https://api.tumblr.com/")
            {
                Authenticator = OAuth1Authenticator.ForProtectedResource(consumerId, consumerSecret, accessToken, accessSecret),
            })
            {
                var request = new RestRequest("v2/blog/{hostName}/post", Method.POST);
                request.AddUrlSegment("hostName", hostName);
                request.AddParameter("type", "text");
                request.AddParameter("body", text);

                var response = await client.Execute(request);
                Assert.NotNull(response);
            }
        }

        private static long ToUnixTime(DateTime dateTime)
        {
            var timeSpan = (dateTime - new DateTime(1970, 1, 1));
            var timestamp = (long)timeSpan.TotalSeconds;
            return timestamp;
        }

        class MyRandomNumberGenerator : IRandom
        {
            /// <summary>
            /// Gets the next random value with <paramref name="minValue"/> &lt;= n &lt; <paramref name="maxValue"/>
            /// </summary>
            /// <param name="minValue">The minimum value (inclusive)</param>
            /// <param name="maxValue">The maximum value (exclusive)</param>
            /// <returns>the next random value</returns>
            public int Next(int minValue, int maxValue)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the next <paramref name="count"/> random values with <paramref name="minValue"/> &lt;= n &lt; <paramref name="maxValue"/>
            /// </summary>
            /// <param name="minValue">The minimum value (inclusive)</param>
            /// <param name="maxValue">The maximum value (exclusive)</param>
            /// <param name="count">The number of random values to generate</param>
            /// <returns>the next random values</returns>
            public int[] Next(int minValue, int maxValue, int count)
            {
                var result = new int[count];
                var range = maxValue - minValue;
                for (var i = 0; i != count; ++i)
                {
                    result[i] = minValue + (i % range);
                }
                return result;
            }
        }

        private class TestRestClient : RestClientBase
        {
            public TestRestClient()
                : base(null)
            {
                BaseUrl = new Uri("https://test.lacolhost/");
            }

            protected override IHttpContent GetContent(IRestRequest request)
            {
                return null;
            }
        }
    }
}

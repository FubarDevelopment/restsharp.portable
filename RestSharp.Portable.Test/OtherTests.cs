using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using RestSharp.Portable.HttpClient;
using RestSharp.Portable.HttpClient.Impl;
using RestSharp.Portable.Test.HttpBin;
using RestSharp.Portable.WebRequest.Impl;

using Xunit;

namespace RestSharp.Portable.Test
{
    public class OtherTests : RestSharpTests
    {
        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestMultipleRequests(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                {
                    var request = new RestRequest("post", Method.POST);
                    request.AddParameter("param1", "param1");

                    var response = await client.Execute<HttpBinResponse>(request);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Form);
                    Assert.True(response.Data.Form.ContainsKey("param1"));
                    Assert.Equal("param1", response.Data.Form["param1"]);
                }

                {
                    var request = new RestRequest("post", Method.POST);
                    request.AddParameter("param1", "param1+");

                    var response = await client.Execute<HttpBinResponse>(request);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Form);
                    Assert.True(response.Data.Form.ContainsKey("param1"));
                    Assert.Equal("param1+", response.Data.Form["param1"]);
                }
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestPutRequestByteArray(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var request = new RestRequest("put", Method.PUT);
                var data = Encoding.UTF8.GetBytes("Hello!");
                request.AddParameter(string.Empty, data, ParameterType.RequestBody);

                var response = await client.Execute<HttpBinResponse>(request);
                Assert.NotNull(response.Data);
                Assert.Equal("Hello!", response.Data.Data);
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestPutRequestJson(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var request = new RestRequest("put", Method.PUT);
                request.AddObject(
                    new
                    {
                        hello = "world",
                    });

                var response = await client.Execute<HttpBinResponse>(request);
                Assert.NotNull(response.Data);
                Assert.NotNull(response.Data.Args);
                Assert.Contains("hello", response.Data.Args.Keys);
                Assert.Equal("world", response.Data.Args["hello"]);
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public void TestUserAgent(Type factoryType)
        {
            var version = typeof(RestClientBase).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            Assert.NotNull(version);

            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                Assert.NotNull(client.UserAgent);
                Assert.Equal(1, client.DefaultParameters.Select(x => x.Name).Count(x => StringComparer.OrdinalIgnoreCase.Equals(x, "User-Agent")));
                Assert.Equal($"RestSharp/{version}", client.UserAgent);
                Assert.Equal($"RestSharp/{version}", (string)client.DefaultParameters.Single(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, "User-Agent")).Value);

                client.UserAgent = "TestUserAgent/1";
                Assert.Equal(1, client.DefaultParameters.Select(x => x.Name).Count(x => StringComparer.OrdinalIgnoreCase.Equals(x, "User-Agent")));
                Assert.Equal("TestUserAgent/1", client.UserAgent);
                Assert.Equal("TestUserAgent/1", (string)client.DefaultParameters.Single(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, "User-Agent")).Value);
            }
        }

        [Fact]
        public void TestCombineRequestUrl()
        {
            using (var client = new RestSharp.Portable.WebRequest.RestClient("https://www.itsg-trust.de/ostc/")
            {
                HttpClientFactory = CreateClientFactory(typeof(WebRequestHttpClientFactory), false),
            })
            {
                var request = new RestRequest(new Uri("http://www.itsg.de/tc_keys_arbeitgeberverfahren.ITSG"));
                var targetUri = client.BuildUri(request);
                Assert.Equal(new Uri("http://www.itsg.de/tc_keys_arbeitgeberverfahren.ITSG"), targetUri);
            }
        }
    }
}

using System;
using System.Threading.Tasks;

using RestSharp.Portable.HttpClient;
using RestSharp.Portable.HttpClient.Impl;
using RestSharp.Portable.Test.HttpBin;
using RestSharp.Portable.WebRequest.Impl;

using Xunit;

namespace RestSharp.Portable.Test
{
    [CLSCompliant(false)]
    public class PostHeaderRequestTests : RestSharpTests
    {
        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestRequestParameter(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var request = new RestRequest("post", Method.POST);
                request.AddHeader("Restsharp-Test1", "TestValue1");
                request.AddParameter("param1", "ParamValue1");

                var response = await client.Execute<PostResponse>(request);
                Assert.Equal("ParamValue1", response.Data.Form["param1"]);
                Assert.Equal("TestValue1", response.Data.Headers["Restsharp-Test1"]);
            }
        }

        [Theory]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestDefaultParameter(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                client.AddDefaultParameter("Restsharp-Test2", "TestValue2", ParameterType.HttpHeader);

                var request = new RestRequest("post", Method.POST);
                request.AddParameter("param1", "ParamValue1");

                var response = await client.Execute<PostResponse>(request);
                Assert.Equal("ParamValue1", response.Data.Form["param1"]);
                Assert.Equal("TestValue2", response.Data.Headers["Restsharp-Test2"]);
            }
        }
    }
}

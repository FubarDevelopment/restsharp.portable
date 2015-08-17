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

                    var response = await client.Execute<PostResponse>(request);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Form);
                    Assert.True(response.Data.Form.ContainsKey("param1"));
                    Assert.Equal("param1", response.Data.Form["param1"]);
                }

                {
                    var request = new RestRequest("post", Method.POST);
                    request.AddParameter("param1", "param1+");

                    var response = await client.Execute<PostResponse>(request);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Form);
                    Assert.True(response.Data.Form.ContainsKey("param1"));
                    Assert.Equal("param1+", response.Data.Form["param1"]);
                }
            }
        }
    }
}

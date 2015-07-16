using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using RestSharp.Portable.HttpClient;

using Xunit;

namespace RestSharp.Portable.Test.HttpClientTests
{
    public class OtherTests
    {
        [Fact]
        public async Task TestMultipleRequests()
        {
            using (var client = new RestClient("http://httpbin.org/"))
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

        // ReSharper disable once ClassNeverInstantiated.Local
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "ReSharper bug")]
        private class PostResponse
        {
            public Dictionary<string, string> Form { get; set; }
        }
    }
}

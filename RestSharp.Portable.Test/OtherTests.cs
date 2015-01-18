using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Portable.Test
{
    public class OtherTests
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        class PostResponse
        {
            public Dictionary<string, string> Form { get; set; }
            public Dictionary<string, string> Headers { get; set; }
        }

        [Fact]
        public async Task TestMultipleRequests()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                {
                    var request = new RestRequest("post", HttpMethod.Post);
                    request.AddParameter("param1", "param1");

                    var response = await client.Execute<PostResponse>(request);
                    Assert.NotNull(response.Data);
                    Assert.NotNull(response.Data.Form);
                    Assert.True(response.Data.Form.ContainsKey("param1"));
                    Assert.Equal("param1", response.Data.Form["param1"]);
                }
                {
                    var request = new RestRequest("post", HttpMethod.Post);
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RestSharp.Portable.Test
{
    public class IssueTests
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
        public async Task TestIssue12_Post1()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var tmp = new string('a', 70000);

                var request = new RestRequest("post", HttpMethod.Post);
                request.AddParameter("param1", tmp);

                var response = await client.Execute<PostResponse>(request);
                Assert.NotNull(response.Data);
                Assert.NotNull(response.Data.Form);
                Assert.True(response.Data.Form.ContainsKey("param1"));
                Assert.Equal(70000, response.Data.Form["param1"].Length);
                Assert.Equal(tmp, response.Data.Form["param1"]);
            }
        }

        [Fact]
        public async Task TestIssue12_Post2()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var tmp = new string('a', 70000);

                var request = new RestRequest("post", HttpMethod.Post);
                request.AddParameter("param1", tmp);
                request.AddParameter("param2", "param2");

                var response = await client.Execute<PostResponse>(request);
                Assert.NotNull(response.Data);
                Assert.NotNull(response.Data.Form);
                Assert.True(response.Data.Form.ContainsKey("param1"));
                Assert.Equal(70000, response.Data.Form["param1"].Length);
                Assert.Equal(tmp, response.Data.Form["param1"]);

                Assert.True(response.Data.Form.ContainsKey("param2"));
                Assert.Equal("param2", response.Data.Form["param2"]);
            }
        }
    }
}

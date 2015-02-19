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
        [Fact(DisplayName = "Issue 12, Post 1 parameter")]
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

        [Fact(DisplayName = "Issue 12, Post 2 parameters")]
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

        [Fact(DisplayName = "Issue 16")]
        public void TestIssue16()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var request = new RestRequest("get?a={a}");
                request.AddParameter("a", "value-of-a", ParameterType.UrlSegment);

                Assert.Equal("http://httpbin.org/get?a=value-of-a", client.BuildUri(request).ToString());
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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using JetBrains.Annotations;

using RestSharp.Portable.Authenticators;

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

                var response = await client.Execute<RequestResponse>(request);
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

                var response = await client.Execute<RequestResponse>(request);
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

        [Fact(DisplayName = "Issue 19")]
        public void TestIssue19()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var req1 = new RestRequest("post", HttpMethod.Post);
                req1.AddParameter("a", "value-of-a");
                var t1 = client.Execute<RequestResponse>(req1);

                var req2 = new RestRequest("post", HttpMethod.Post);
                req2.AddParameter("ab", "value-of-ab");
                var t2 = client.Execute<RequestResponse>(req2);

                Task.WaitAll(t1, t2);

                Assert.NotNull(t1.Result.Data);
                Assert.NotNull(t1.Result.Data.Form);
                Assert.True(t1.Result.Data.Form.ContainsKey("a"));
                Assert.Equal("value-of-a", t1.Result.Data.Form["a"]);

                Assert.NotNull(t2.Result.Data);
                Assert.NotNull(t2.Result.Data.Form);
                Assert.True(t2.Result.Data.Form.ContainsKey("ab"));
                Assert.Equal("value-of-ab", t2.Result.Data.Form["ab"]);
            }
        }

        [Fact(DisplayName = "Issue 23")]
        public async Task TestIssue23()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                client.Authenticator = new HttpBasicAuthenticator("foo", "bar");
                var request = new RestRequest("post", HttpMethod.Get);
                request.AddJsonBody("foo");
                await client.Execute(request);
            }
        }

        [Fact(DisplayName = "Issue 25")]
        public void TestIssue25()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var req1 = new RestRequest("post", HttpMethod.Post);
                req1.AddParameter("a", "value-of-a");

                var req2 = new RestRequest("post", HttpMethod.Post);
                req2.AddParameter("ab", "value-of-ab");

                var t1 = client.Execute<RequestResponse>(req1);
                var t2 = client.Execute<RequestResponse>(req2);

                Task.WaitAll(t1, t2);

                Assert.NotNull(t1.Result.Data);
                Assert.NotNull(t1.Result.Data.Form);
                Assert.True(t1.Result.Data.Form.ContainsKey("a"));
                Assert.Equal("value-of-a", t1.Result.Data.Form["a"]);

                Assert.NotNull(t2.Result.Data);
                Assert.NotNull(t2.Result.Data.Form);
                Assert.True(t2.Result.Data.Form.ContainsKey("ab"));
                Assert.Equal("value-of-ab", t2.Result.Data.Form["ab"]);
            }
        }

        [Fact(DisplayName = "Issue 32")]
        public async Task TestIssue32()
        {
            using (var client = new RestClient("http://httpbin.org/cookies")
            {
                CookieContainer = new CookieContainer(),
            })
            {
                {
                    var req = new RestRequest(string.Empty);
                    var response = await client.Execute<RequestResponse>(req);
                    Assert.Equal(0, response.Data.Cookies.Count);
                }
                {
                    Assert.Equal(0, client.CookieContainer.Count);
                }
                {
                    var req = new RestRequest("set");
                    req.AddParameter("k1", "v1");
                    var response = await client.Execute<RequestResponse>(req);
                    Assert.Collection(
                        response.Data.Cookies,
                        kvp =>
                        {
                            Assert.Equal("k1", kvp.Key);
                            Assert.Equal("v1", kvp.Value);
                        });
                }
                {
                    Assert.Equal(1, client.CookieContainer.Count);
                }
                {
                    var req = new RestRequest();
                    var response = await client.Execute<RequestResponse>(req);
                    Assert.Collection(
                        response.Data.Cookies,
                        kvp =>
                        {
                            Assert.Equal("k1", kvp.Key);
                            Assert.Equal("v1", kvp.Value);
                        });
                }
                {
                    Assert.Equal(1, client.CookieContainer.Count);
                }
                {
                    var req = new RestRequest("set");
                    req.AddParameter("k2", "v2");
                    var response = await client.Execute<RequestResponse>(req);
                    Assert.Collection(
                        response.Data.Cookies,
                        kvp =>
                        {
                            Assert.Equal("k1", kvp.Key);
                            Assert.Equal("v1", kvp.Value);
                        },
                        kvp =>
                        {
                            Assert.Equal("k2", kvp.Key);
                            Assert.Equal("v2", kvp.Value);
                        });
                }
                {
                    Assert.Equal(2, client.CookieContainer.Count);
                }
                {
                    var req = new RestRequest();
                    var response = await client.Execute<RequestResponse>(req);
                    Assert.Collection(
                        response.Data.Cookies,
                        kvp =>
                        {
                            Assert.Equal("k1", kvp.Key);
                            Assert.Equal("v1", kvp.Value);
                        },
                        kvp =>
                        {
                            Assert.Equal("k2", kvp.Key);
                            Assert.Equal("v2", kvp.Value);
                        });
                }
                {
                    Assert.Equal(2, client.CookieContainer.Count);
                }
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class RequestResponse
        {
            public Dictionary<string, string> Form { get; set; }
            public Dictionary<string, string> Cookies { get; set; }
        }
    }
}

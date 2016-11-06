using System;
using System.Net;
using System.Threading.Tasks;

using RestSharp.Portable.Authenticators;
using RestSharp.Portable.HttpClient;
using RestSharp.Portable.HttpClient.Impl;
using RestSharp.Portable.Tests.HttpBin;
using RestSharp.Portable.WebRequest.Impl;

using Xunit;

namespace RestSharp.Portable.Tests
{
    public class IssueTests : RestSharpTests
    {
        [Theory(DisplayName = "Issue 12, Post 1 parameter")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue12_Post1(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var tmp = new string('a', 70000);

                var request = new RestRequest("post", Method.POST);
                request.AddParameter("param1", tmp);

                var response = await client.Execute<HttpBinResponse>(request);
                Assert.NotNull(response.Data);
                Assert.NotNull(response.Data.Form);
                Assert.True(response.Data.Form.ContainsKey("param1"));
                Assert.Equal(70000, response.Data.Form["param1"].Length);
                Assert.Equal(tmp, response.Data.Form["param1"]);
            }
        }

        [Theory(DisplayName = "Issue 12, Post 2 parameters")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue12_Post2(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var tmp = new string('a', 70000);

                var request = new RestRequest("post", Method.POST);
                request.AddParameter("param1", tmp);
                request.AddParameter("param2", "param2");

                var response = await client.Execute<HttpBinResponse>(request);
                Assert.NotNull(response.Data);
                Assert.NotNull(response.Data.Form);
                Assert.True(response.Data.Form.ContainsKey("param1"));
                Assert.Equal(70000, response.Data.Form["param1"].Length);
                Assert.Equal(tmp, response.Data.Form["param1"]);

                Assert.True(response.Data.Form.ContainsKey("param2"));
                Assert.Equal("param2", response.Data.Form["param2"]);
            }
        }

        [Theory(DisplayName = "Issue 16")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public void TestIssue16(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var request = new RestRequest("get?a={a}");
                request.AddParameter("a", "value-of-a", ParameterType.UrlSegment);

                Assert.Equal("http://httpbin.org/get?a=value-of-a", client.BuildUri(request).ToString());
            }
        }

        [Theory(DisplayName = "Issue 19")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public void TestIssue19(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var req1 = new RestRequest("post", Method.POST);
                req1.AddParameter("a", "value-of-a");
                var t1 = client.Execute<HttpBinResponse>(req1);

                var req2 = new RestRequest("post", Method.POST);
                req2.AddParameter("ab", "value-of-ab");
                var t2 = client.Execute<HttpBinResponse>(req2);

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

        [Theory(DisplayName = "Issue 23")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue23(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                client.Authenticator = new HttpBasicAuthenticator();
                client.Credentials = new NetworkCredential("foo", "bar");
                var request = new RestRequest("post", Method.GET);
                request.AddJsonBody("foo");
                await client.Execute(request);
            }
        }

        [Theory(DisplayName = "Issue 25")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public void TestIssue25(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var req1 = new RestRequest("post", Method.POST);
                req1.AddParameter("a", "value-of-a");

                var req2 = new RestRequest("post", Method.POST);
                req2.AddParameter("ab", "value-of-ab");

                var t1 = client.Execute<HttpBinResponse>(req1);
                var t2 = client.Execute<HttpBinResponse>(req2);
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

        [Theory(DisplayName = "Issue 29 ContentCollectionMode = MultiPart")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue29_CollectionModeMultiPart(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var req = new RestRequest("post", Method.POST);
                req.AddParameter("a", "value-of-a");
                req.ContentCollectionMode = ContentCollectionMode.MultiPart;
                var resp = await client.Execute<HttpBinResponse>(req);
                Assert.NotNull(resp.Data);
                Assert.NotNull(resp.Data.Form);
                Assert.True(resp.Data.Form.ContainsKey("a"));
                Assert.Equal("value-of-a", resp.Data.Form["a"]);
            }
        }

        [Theory(DisplayName = "Issue 29 ContentType as Parameter")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue29_ContentTypeParameter(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
            })
            {
                var req = new RestRequest("post", Method.POST);
                req.AddParameter("a", "value-of-a");
                req.AddHeader("content-type", "application/x-www-form-urlencoded;charset=utf-8");
                var resp = await client.Execute<HttpBinResponse>(req);
                Assert.NotNull(resp.Data);
                Assert.NotNull(resp.Data.Form);
                Assert.True(resp.Data.Form.ContainsKey("a"));
                Assert.Equal("value-of-a", resp.Data.Form["a"]);
            }
        }

        [Theory(DisplayName = "Issue 53", Skip = "Cannot reproduce this problem")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue53(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
                IgnoreResponseStatusCode = true,
            })
            {
                var req = new RestRequest("get", Method.GET);
                var resp = await client.Execute<HttpBinResponse>(req);
                Assert.Null(resp.Data);
            }
        }

        [Theory(DisplayName = "Issue 85")]
        [InlineData(typeof(DefaultHttpClientFactory))]
        [InlineData(typeof(WebRequestHttpClientFactory))]
        public async Task TestIssue85(Type factoryType)
        {
            using (var client = new RestClient("http://httpbin.org/")
            {
                HttpClientFactory = CreateClientFactory(factoryType, false),
                IgnoreResponseStatusCode = true,
                CookieContainer = new CookieContainer(),
            })
            {
                var req = new RestRequest("cookies/set", Method.GET);
                req.AddQueryParameter("n1", "v1");
                var resp = await client.Execute<HttpBinResponse>(req);
                Assert.NotNull(resp.Data.Cookies);
                Assert.Equal(1, resp.Data.Cookies.Count);
                Assert.Equal("v1", resp.Data.Cookies["n1"]);

                Assert.NotNull(resp.Cookies);
                Assert.Equal(1, resp.Cookies.Count);
                Assert.NotNull(resp.Cookies["n1"]);
                Assert.Equal("v1", resp.Cookies["n1"].Value);
            }
        }
    }
}

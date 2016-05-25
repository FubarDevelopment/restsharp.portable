using System;
using System.Threading.Tasks;

using RestSharp.Portable.HttpClient;

using Xunit;

namespace RestSharp.Portable.Tests
{
    public class RequestParameterTests
    {
        [Fact]
        public void TestParameterCaseSensitive()
        {
            using (var client = new RestClient("http://localhost"))
            {
                client.AddDefaultParameter("param1", "value1");

                var request = new RestRequest();
                request
                    .AddParameter("param2", "value2")
                    .AddParameter("Param1", "value1.1");

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1&param2=value2&Param1=value1.1", uri);
            }
        }

        [Fact]
        public void TestOnlyDefaultParameter()
        {
            using (var client = new RestClient("http://localhost"))
            {
                client.AddDefaultParameter("param1", "value1");

                var request = new RestRequest();

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1", uri);
            }
        }

        [Fact]
        public void TestParameterCaseInsensitive()
        {
            using (var client = new RestClient("http://localhost"))
            {
                client.AddDefaultParameter("param1", "value1");

                var request = new RestRequest();
                request
                    .AddParameter("param2", "value2")
                    .AddParameter("Param1", "value1.1");

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1&param2=value2&Param1=value1.1", uri);
            }
        }

        [Fact]
        public void TestParameterOverride()
        {
            using (var client = new RestClient("http://localhost"))
            {
                client.AddDefaultParameter("param1", "value1");

                var request = new RestRequest();
                request
                    .AddParameter("param2", "value2")
                    .AddOrUpdateParameter("param1", "value1.1");

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1.1&param2=value2", uri);
            }
        }

        [Fact]
        public void TestParameterDuplication()
        {
            using (var client = new RestClient("http://localhost"))
            {
                var request = new RestRequest();
                request
                    .AddParameter("param1", "value1")
                    .AddParameter("param2", "value2")
                    .AddOrUpdateParameter("param1", "value1.1");

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1.1&param2=value2", uri);
            }
        }

        [Fact]
        public void TestParameterDuplicationGetAndQueryString()
        {
            using (var client = new RestClient("http://localhost"))
            {
                var request = new RestRequest();
                request
                    .AddParameter("param1", "value1", ParameterType.GetOrPost)
                    .AddParameter("param2", "value2")
                    .AddOrUpdateParameter("param1", "value1.1", ParameterType.QueryString);

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1&param2=value2&param1=value1.1", uri);
            }
        }

        [Fact]
        public async Task TestParameterDuplicationPostAndQueryString()
        {
            using (var client = new RestClient("http://localhost"))
            {
                var request = new RestRequest(Method.POST);
                request
                    .AddParameter("param1", "value1", ParameterType.GetOrPost)
                    .AddParameter("param2", "value2")
                    .AddOrUpdateParameter("param1", "value1.1", ParameterType.QueryString);

                var uri = client.BuildUri(request).ToString();
                Assert.Equal("http://localhost/?param1=value1.1", uri);

                var requestParameters = client.MergeParameters(request);
                var body = await client.GetContent(request, requestParameters).ReadAsStringAsync();
                Assert.Equal("param1=value1&param2=value2", body);
            }
        }
    }
}

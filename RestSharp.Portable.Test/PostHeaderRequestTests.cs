using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Xunit;

namespace RestSharp.Portable.Test
{
    public class PostHeaderRequestTests
    {
        [Fact]
        public async Task TestRequestParameter()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var request = new RestRequest("post", Method.POST);
                request.AddHeader("Restsharp-Test1", "TestValue1");
                request.AddParameter("param1", "ParamValue1");

                var response = await client.Execute<Response>(request);
                Assert.Equal("ParamValue1", response.Data.Form["param1"]);
                Assert.Equal("TestValue1", response.Data.Headers["Restsharp-Test1"]);
            }
        }

        [Fact]
        public async Task TestDefaultParameter()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                client.AddDefaultParameter("Restsharp-Test2", "TestValue2", ParameterType.HttpHeader);

                var request = new RestRequest("post", Method.POST);
                request.AddParameter("param1", "ParamValue1");

                var response = await client.Execute<Response>(request);
                Assert.Equal("ParamValue1", response.Data.Form["param1"]);
                Assert.Equal("TestValue2", response.Data.Headers["Restsharp-Test2"]);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local", Justification = "Is updated by RestSharp")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "ReSharper Bug")]
        private class Response
        {
            public Dictionary<string, string> Form { get; set; }

            public Dictionary<string, string> Headers { get; set; }
        }
    }
}

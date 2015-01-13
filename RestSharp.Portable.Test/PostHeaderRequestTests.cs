using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class PostHeaderRequestTests
    {
        class Response
        {
            public Dictionary<string, string> Form { get; set; }
            public Dictionary<string, string> Headers { get; set; }
        }

        [TestMethod]
        public async Task TestRequestParameter()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var request = new RestRequest("post", HttpMethod.Post);
                request.AddHeader("Restsharp-Test1", "TestValue1");
                request.AddParameter("param1", "ParamValue1");

                var response = await client.Execute<Response>(request);
                Assert.AreEqual("ParamValue1", response.Data.Form["param1"]);
                Assert.AreEqual("TestValue1", response.Data.Headers["Restsharp-Test1"]);
            }
        }

        [TestMethod]
        public async Task TestDefaultParameter()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                client.AddDefaultParameter("Restsharp-Test2", "TestValue2", ParameterType.HttpHeader);

                var request = new RestRequest("post", HttpMethod.Post);
                request.AddParameter("param1", "ParamValue1");

                var response = await client.Execute<Response>(request);
                Assert.AreEqual("ParamValue1", response.Data.Form["param1"]);
                Assert.AreEqual("TestValue2", response.Data.Headers["Restsharp-Test2"]);
            }
        }
    }
}

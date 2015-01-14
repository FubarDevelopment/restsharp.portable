using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class IssueTests
    {
        class PostResponse
        {
            public Dictionary<string, string> Form { get; set; }
            public Dictionary<string, string> Headers { get; set; }
        }

        [TestMethod]
        public async Task TestIssue12_Post1()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var tmp = new string('a', 70000);

                var request = new RestRequest("post", HttpMethod.Post);
                request.AddParameter("param1", tmp);

                var response = await client.Execute<PostResponse>(request);
                Assert.IsNotNull(response.Data);
                Assert.IsNotNull(response.Data.Form);
                Assert.IsTrue(response.Data.Form.ContainsKey("param1"));
                Assert.AreEqual(70000, response.Data.Form["param1"].Length);
                Assert.AreEqual(tmp, response.Data.Form["param1"]);
            }
        }

        [TestMethod]
        public async Task TestIssue12_Post2()
        {
            using (var client = new RestClient("http://httpbin.org/"))
            {
                var tmp = new string('a', 70000);

                var request = new RestRequest("post", HttpMethod.Post);
                request.AddParameter("param1", tmp);
                request.AddParameter("param2", "param2");

                var response = await client.Execute<PostResponse>(request);
                Assert.IsNotNull(response.Data);
                Assert.IsNotNull(response.Data.Form);
                Assert.IsTrue(response.Data.Form.ContainsKey("param1"));
                Assert.AreEqual(70000, response.Data.Form["param1"].Length);
                Assert.AreEqual(tmp, response.Data.Form["param1"]);

                Assert.IsTrue(response.Data.Form.ContainsKey("param2"));
                Assert.AreEqual("param2", response.Data.Form["param2"]);
            }
        }
    }
}

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
        public async Task TestIssue12()
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
    }
}

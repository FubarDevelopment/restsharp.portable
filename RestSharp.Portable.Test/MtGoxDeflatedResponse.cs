using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class MtGoxDeflatedResponse
    {
        [TestMethod]
        public async Task TestGetTicker()
        {
            var client = new RestClient(new Uri("https://data.mtgox.com/api/2/"));
            client.AddEncoding("GZIP", new RestSharp.Portable.Encodings.GzipEncoding());
            var request = new RestRequest("BTC{currency}/money/ticker", HttpMethod.Post);
            request.AddUrlSegment("currency", BitCoin.Trade.MtGox.Currency.USD);
            var tmp = await client.Execute(request);
        }
    }
}

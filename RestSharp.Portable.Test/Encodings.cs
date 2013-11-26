using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class Encodings
    {
        [TestMethod]
        public async Task TestGetMtGoxTicker()
        {
            var client = new RestClient(new Uri("https://data.mtgox.com/api/2/"));
            client.AddEncoding("GZIP", new RestSharp.Portable.Encodings.GzipEncoding());
            //client.IgnoreResponseStatusCode = true;
            var request = new RestRequest("BTC{currency}/money/ticker", HttpMethod.Get);
            request.AddUrlSegment("currency", BitCoin.Trade.MtGox.Currency.USD);
            var tmp = await client.Execute(request);
            System.Diagnostics.Debug.WriteLine(System.Text.Encoding.UTF8.GetString(tmp.RawBytes));
        }
    }
}

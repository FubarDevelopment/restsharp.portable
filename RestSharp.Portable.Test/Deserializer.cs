using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class Deserializer
    {
        class CustomDeserializer : RestSharp.Portable.Deserializers.JsonDeserializer
        {
            protected override void ConfigureSerializer(Newtonsoft.Json.JsonSerializer serializer)
            {
                base.ConfigureSerializer(serializer);
                serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            }
        }

        [TestMethod]
        public void TestReplaceContentTypeDeserializer()
        {
            var restClient = new RestClient();
            var deserializer = new CustomDeserializer();
            restClient.ReplaceHandler(typeof(RestSharp.Portable.Deserializers.IDeserializer), deserializer);
            Assert.AreSame(deserializer, restClient.GetHandler("application/json"));
            Assert.AreSame(deserializer, restClient.GetHandler("text/json"));
        }
    }
}

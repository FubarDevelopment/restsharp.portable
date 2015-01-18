using Xunit;

namespace RestSharp.Portable.Test
{
    public class CustomDeserializer
    {
        class TestDeserializer : Deserializers.JsonDeserializer
        {
            protected override void ConfigureSerializer(Newtonsoft.Json.JsonSerializer serializer)
            {
                base.ConfigureSerializer(serializer);
                serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            }
        }

        [Fact]
        public void TestReplaceContentTypeDeserializer()
        {
            var restClient = new RestClient();
            var deserializer = new TestDeserializer();
            restClient.ReplaceHandler(typeof(Deserializers.JsonDeserializer), deserializer);
            Assert.Same(deserializer, restClient.GetHandler("application/json"));
            Assert.Same(deserializer, restClient.GetHandler("text/json"));
        }
    }
}

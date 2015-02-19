using System;
using System.Runtime.Serialization;
using RestSharp.Portable.Serializers;

using Xunit;

namespace RestSharp.Portable.Test
{
    public class SerializerTests
    {
        [Fact]
        public void TestXmlSerializerBom()
        {
            var serializer = new XmlDataContractSerializer();
            var input = new TestData { TestProp = "propValue1 öäüß" };
            var output = serializer.Serialize(input);
            var lastThreeBytes = new byte[3];
            Array.Copy(output, output.Length - 3, lastThreeBytes, 0, 3);
            Assert.NotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, lastThreeBytes);

            var firstThreeBytes = new byte[3];
            Array.Copy(output, 0, firstThreeBytes, 0, 3);
            Assert.NotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, firstThreeBytes);
        }

        [DataContract(Namespace = "")]
        private class TestData
        {
            [DataMember]
            public string TestProp { get; set; }
        }
    }
}

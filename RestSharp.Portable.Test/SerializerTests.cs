using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;
using System.Linq;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class SerializerTests
    {
        [DataContract(Namespace = "")]
        class TestData
        {
            [DataMember]
            public string TestProp { get; set; }
        }

        [TestMethod]
        public void TestXmlSerializerBom()
        {
            var serializer = new RestSharp.Portable.Serializers.XmlDataContractSerializer();
            var input = new TestData { TestProp = "propValue1 öäüß" };
            var output = serializer.Serialize(input);
            var lastThreeBytes = new byte[3];
            Array.Copy(output, output.Length - 3, lastThreeBytes, 0, 3);
            CollectionAssert.AreNotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, lastThreeBytes);

            var firstThreeBytes = new byte[3];
            Array.Copy(output, 0, firstThreeBytes, 0, 3);
            CollectionAssert.AreNotEqual(new byte[] { 0xEF, 0xBB, 0xBF }, firstThreeBytes);
        }
    }
}

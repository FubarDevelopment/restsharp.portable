//#define ENABLE_MULTI_TEST
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Web;

namespace RestSharp.Portable.Test
{
    [TestClass]
    public class UrlEncodeTests
    {
        [TestMethod]
        public void TestEscapeDataStringComplianceASCII()
        {
            var chars = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            var expected = Uri.EscapeDataString(chars);
            var test = UrlUtility.Escape(chars);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void TestEscapeDataStringComplianceUmlaut()
        {
            var chars = "äöüßÄÖÜ\u007F";
            var expected = Uri.EscapeDataString(chars);
            var test = UrlUtility.Escape(chars);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void TestUrlEncodeComplianceASCII()
        {
            var chars = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            var expected = HttpUtility.UrlEncode(chars);
            var test = UrlUtility.Escape(chars, UrlEscapeFlags.LikeUrlEncode);
            Assert.AreEqual(expected, test);
        }

        [TestMethod]
        public void TestUrlEncodeComplianceUmlaut()
        {
            var chars = "äöüßÄÖÜ\u007F";
            var expected = HttpUtility.UrlEncode(chars);
            var test = UrlUtility.Escape(chars, UrlEscapeFlags.LikeUrlEncode);
            Assert.AreEqual(expected, test);
        }

#if ENABLE_MULTI_TEST
        [TestMethod]
        public void TestEscapeDataStringComplianceASCII100000()
        {
            for (int i = 0; i != 100000; ++i)
                TestEscapeDataStringComplianceASCII();
        }

        [TestMethod]
        public void TestUrlEncodeComplianceASCII100000()
        {
            for (int i = 0; i != 100000; ++i)
                TestUrlEncodeComplianceASCII();
        }
#endif
    }
}

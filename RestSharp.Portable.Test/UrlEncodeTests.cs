//#define ENABLE_MULTI_TEST
using System;
using System.Linq;
using System.Web;
using Xunit;

namespace RestSharp.Portable.Test
{
    public class UrlEncodeTests
    {
        [Fact]
        public void TestEscapeDataStringComplianceASCII()
        {
            var chars = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            var expected = Uri.EscapeDataString(chars);
            var test = UrlUtility.Escape(chars);
            Assert.Equal(expected, test);
        }

        [Fact]
        public void TestEscapeDataStringComplianceUmlaut()
        {
            const string chars = "äöüßÄÖÜ\u007F";
            var expected = Uri.EscapeDataString(chars);
            var test = UrlUtility.Escape(chars);
            Assert.Equal(expected, test);
        }

        [Fact]
        public void TestUrlEncodeComplianceASCII()
        {
            var chars = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            var expected = HttpUtility.UrlEncode(chars);
            var test = UrlUtility.Escape(chars, UrlEscapeFlags.LikeUrlEncode);
            Assert.Equal(expected, test);
        }

        [Fact]
        public void TestUrlEncodeComplianceUmlaut()
        {
            const string chars = "äöüßÄÖÜ\u007F";
            var expected = HttpUtility.UrlEncode(chars);
            var test = UrlUtility.Escape(chars, UrlEscapeFlags.LikeUrlEncode);
            Assert.Equal(expected, test);
        }

#if ENABLE_MULTI_TEST
        [Fact]
        public void TestEscapeDataStringComplianceASCII100000()
        {
            for (int i = 0; i != 100000; ++i)
                TestEscapeDataStringComplianceASCII();
        }

        [Fact]
        public void TestUrlEncodeComplianceASCII100000()
        {
            for (int i = 0; i != 100000; ++i)
                TestUrlEncodeComplianceASCII();
        }
#endif
    }
}

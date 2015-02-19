using System;
using System.Linq;
using System.Web;

using Xunit;

namespace RestSharp.Portable.Test
{
    public class UrlEncodeVariant2Tests
    {
        private readonly UrlEscapeUtility _utility = new UrlEscapeUtility(false);

        [Fact]
        public void TestEscapeDataStringComplianceASCII()
        {
            const UrlEscapeFlags flags = UrlEscapeFlags.BuilderVariantListByteArray;
            var chars = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            var expected = Uri.EscapeDataString(chars);
            var test = _utility.Escape(chars, flags);
            Assert.Equal(expected, test);
            Assert.Equal(expected.Length, _utility.ComputeLength(chars, flags));
        }

        [Fact]
        public void TestEscapeDataStringComplianceUmlaut()
        {
            const UrlEscapeFlags flags = UrlEscapeFlags.BuilderVariantListByteArray;
            const string chars = "äöüßÄÖÜ\u007F";
            var expected = Uri.EscapeDataString(chars);
            var test = _utility.Escape(chars, flags);
            Assert.Equal(expected, test);
            Assert.Equal(expected.Length, _utility.ComputeLength(chars, flags));
        }

        [Fact]
        public void TestUrlEncodeComplianceASCII()
        {
            const UrlEscapeFlags flags = UrlEscapeFlags.LikeUrlEncode | UrlEscapeFlags.BuilderVariantListByteArray;
            var chars = new string(Enumerable.Range(32, 95).Select(x => (char)x).ToArray());
            var expected = HttpUtility.UrlEncode(chars);
            Assert.NotNull(expected);
            var test = _utility.Escape(chars, flags);
            Assert.Equal(expected, test);
            Assert.Equal(expected.Length, _utility.ComputeLength(chars, flags));
        }

        [Fact]
        public void TestUrlEncodeComplianceUmlaut()
        {
            const UrlEscapeFlags flags = UrlEscapeFlags.LikeUrlEncode | UrlEscapeFlags.BuilderVariantListByteArray;
            const string chars = "äöüßÄÖÜ\u007F";
            var expected = HttpUtility.UrlEncode(chars);
            Assert.NotNull(expected);
            var test = _utility.Escape(chars, flags);
            Assert.Equal(expected, test);
            Assert.Equal(expected.Length, _utility.ComputeLength(chars, flags));
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

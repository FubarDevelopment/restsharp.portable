using RestSharp.Portable.Authenticators.OAuth.SignatureProviders;

using Xunit;

namespace RestSharp.Portable.OAuth1.Tests
{
    public class RestSharpIssueTests
    {
        [Fact]
        public void Issue645()
        {
            Assert.Equal("HMAC-SHA1", new HmacSha1SignatureProvider().Id);
        }
    }
}

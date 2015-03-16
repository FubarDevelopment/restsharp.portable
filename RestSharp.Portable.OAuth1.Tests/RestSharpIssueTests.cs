using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp.Portable.Authenticators.OAuth;
using RestSharp.Portable.Authenticators.OAuth.Extensions;

using Xunit;

namespace RestSharp.Portable.OAuth1.Tests
{
    public class RestSharpIssueTests
    {
        [Fact]
        public void Issue645()
        {
            Assert.Equal("HMAC-SHA1", OAuthSignatureMethod.HmacSha1.ToRequestValue());
        }
    }
}

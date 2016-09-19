using System;
using NUnit.Framework;
using RestSharp.Portable.OAuth2;
using RestSharp.Portable.OAuth2.Client;
using RestSharp.Portable.OAuth2.Infrastructure;
using FluentAssertions;

namespace RestSharp.Portable.OAuth2.Tests.Infrastructure
{
    [TestFixture]
    public class SafeExtensionsTests
    {
        [Test]
        public void Should_NotThrow_WhenSafeGetIsCalledOnNull()
        {
            // act & assert
            ((IClient)null).Invoking(x => x.SafeGet(z => z.GetLoginLinkUri()))
                .ShouldNotThrow<NullReferenceException>();
            ((IClient)null).SafeGet(x => x.GetLoginLinkUri()).Should().Be(null);
        }

        [Test]
        public void Should_ReturnResultObtainedFromSelector_WhenSafeGetIsCalledOnInstance()
        {
            // arrange
            const string value = "abc";
            Func<string, string> selector = x => x.Substring(1);

            // act & assert
            value.SafeGet(selector).Should().Be("bc");
        }
    }
}
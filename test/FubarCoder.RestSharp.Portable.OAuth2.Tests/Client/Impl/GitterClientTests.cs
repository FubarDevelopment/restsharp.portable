using System;

using FluentAssertions;

using NSubstitute;

using NUnit.Framework;

using RestSharp.Portable.OAuth2;
using RestSharp.Portable.OAuth2.Client;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;

namespace RestSharp.Portable.OAuth2.Tests.Client.Impl
{
    [TestFixture]
    public class GitterClientTests
    {
        private ClientDescendant _descendant;

        [SetUp]
        public void SetUp()
        {
            _descendant = new ClientDescendant(Substitute.For<IRequestFactory>(), Substitute.For<IClientConfiguration>());
        }

        [Test]
        public void Should_ReturnCorrectAccessCodeServiceEndpoint()
        {
            // act
            var endpoint = _descendant.GetAccessCodeServiceEndpoint();

            // assert
            endpoint.BaseUri.Should().Be("https://gitter.im");
            endpoint.Resource.Should().Be("/login/oauth/authorize");
        }

        [Test]
        public void Should_ReturnCorrectAccessTokenServiceEndpoint()
        {
            // act
            var endpoint = _descendant.GetAccessTokenServiceEndpoint();

            // assert
            endpoint.BaseUri.Should().Be("https://gitter.im");
            endpoint.Resource.Should().Be("/login/oauth/token");
        }

        private class ClientDescendant : GitterClient
        {
            public ClientDescendant(IRequestFactory factory, IClientConfiguration configuration)
                : base(factory, configuration)
            {
            }

            public Endpoint GetAccessCodeServiceEndpoint()
            {
                return AccessCodeServiceEndpoint;
            }

            public Endpoint GetAccessTokenServiceEndpoint()
            {
                return AccessTokenServiceEndpoint;
            }
        }
    }
}

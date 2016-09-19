﻿using FluentAssertions;

using NUnit.Framework;

namespace RestSharp.Portable.OAuth2.Tests.Client
{
    [TestFixture]
    public class EndpointTests
    {
        [Test]
        [TestCase("https://base.com", "/resource", "https://base.com/resource")]
        public void Should_ReturnCompleteUri_AsCombinationOf_BaseUriAndResource(
            string baseUri, string resource, string uri)
        {
            new Endpoint
            {
                BaseUri = baseUri,
                Resource = resource
            }.Uri.Should().Be(uri);
        }
    }
}

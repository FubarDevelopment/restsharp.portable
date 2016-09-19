using FluentAssertions;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Tests.Helpers;

namespace RestSharp.Portable.OAuth2.Tests.Configuration
{
    [TestFixture]
    public class OAuth2ConfigurationSectionTests
    {
        private IConfigurationRoot _configuration;

        [SetUp]
        public void SetUp()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("settings.json");
            _configuration = builder.Build();
        }

        [Test]
        public void Should_ContainAllServiceDefinitions()
        {
            var section = new OAuth2ConfigurationSection();

            // act
            _configuration.GetSection("oauth2").Bind(section);

            // assert
            section["SomeClient"].Should().NotBeNull();
            section["SomeAnotherClient"].Should().NotBeNull();
        }

        [Test]
        public void Should_CorrectlyParseServiceDefinition()
        {
            var section = new OAuth2ConfigurationSection();

            // act
            _configuration.GetSection("oauth2").Bind(section);
            var service = section["SomeAnotherClient"];

            // assert
            service.ClientTypeName.Should().Be("SomeAnotherClient");
            service.ClientId.Should().Be("SomeAnotherClientId");
            service.ClientSecret.Should().Be("SomeAnotherClientSecret");
            service.Scope.Should().Be("SomeAnotherScope");
            service.RedirectUri.Should().Be("https://some-another-redirect-uri.net");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using FizzWare.NBuilder;

using FluentAssertions;

using NSubstitute;
using NUnit.Framework;

using RestSharp.Portable.OAuth1;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

namespace RestSharp.Portable.OAuth2.Tests.Client
{
    [TestFixture]
    public class OAuthClientTests
    {
        private IRequestFactory _factory;
        private OAuthClientDescendant _descendant;

        private static readonly System.Text.Encoding _encoding = System.Text.Encoding.UTF8;

        [SetUp]
        public void SetUp()
        {
            _factory = Substitute.For<IRequestFactory>();
            var client = Substitute.For<IRestClient>();
            var request = Substitute.For<IRestRequest>();
            var response = Substitute.For<IRestResponse>();
            _factory.CreateClient().Returns(client);
            _factory.CreateRequest(Arg.Any<string>()).ReturnsForAnyArgs(request);
            client.Execute(request).Returns(Task.FromResult(response));
            response.StatusCode.Returns(HttpStatusCode.OK);
            _descendant = new OAuthClientDescendant(
                _factory, Substitute.For<IClientConfiguration>());
        }

        [Test]
        public void Should_ThrowNotSupported_When_UserWantsToTransmitState()
        {
            _descendant.Awaiting(x => x.GetLoginLinkUri("any state")).ShouldThrow<NotSupportedException>();
        }

        [Test]
        public async Task Should_ThrowUnexpectedResponse_When_StatusIsNotOk()
        {
            (await _factory.CreateClient().Execute(_factory.CreateRequest(null)).ConfigureAwait(false)).StatusCode.Returns(HttpStatusCode.InternalServerError);
            _descendant.Awaiting(x => x.GetLoginLinkUri()).ShouldThrow<UnexpectedResponseException>();
        }

        [Test]
        public async Task Should_ThrowUnexpectedResponse_When_ContentIsEmpty()
        {
            (await _factory.CreateClient().Execute(_factory.CreateRequest(null)).ConfigureAwait(false)).RawBytes.Returns(_encoding.GetBytes(""));
            _descendant.Awaiting(x => x.GetLoginLinkUri()).ShouldThrow<UnexpectedResponseException>();
        }

        [Test]
        public async Task Should_ThrowUnexpectedResponse_When_OAuthTokenIsEmpty()
        {
            (await _factory.CreateClient().Execute(_factory.CreateRequest(null)).ConfigureAwait(false)).RawBytes.Returns(_encoding.GetBytes("something=something_other"));
            _descendant
                .Awaiting(x => x.GetLoginLinkUri())
                .ShouldThrow<UnexpectedResponseException>()
                .And.FieldName.Should().Be("oauth_token");
        }

        [Test]
        public async Task Should_ThrowUnexpectedResponse_When_OAuthSecretIsEmpty()
        {
            var response = await _factory.CreateClient().Execute(_factory.CreateRequest(null)).ConfigureAwait(false);
            response.RawBytes.Returns(_encoding.GetBytes("oauth_token=token"));
            response.Content.Returns("oauth_token=token");
            _descendant
                .Awaiting(x => x.GetLoginLinkUri())
                .ShouldThrow<UnexpectedResponseException>()
                .And.FieldName.Should().Be("oauth_token_secret");
        }

        [Test]
        public async Task Should_IssueCorrectRequestForRequestToken_When_GetLoginLinkUriIsCalled()
        {
            // arrange
            var restClient = _factory.CreateClient();
            var restRequest = _factory.CreateRequest(null);
            var response = await restClient.Execute(restRequest).ConfigureAwait(false);
            response.RawBytes.Returns(_encoding.GetBytes("any content to pass response verification"));
            response.Content.Returns("oauth_token=token&oauth_token_secret=secret");

            // act
            await _descendant.GetLoginLinkUri().ConfigureAwait(false);

            // assert
            _factory.Received().CreateClient();
            _factory.Received().CreateRequest("/RequestTokenServiceEndpoint");

            restClient.Received().BaseUrl = new Uri("https://RequestTokenServiceEndpoint");
            restRequest.Received().Method = Method.POST;

            restClient.Authenticator.Should().NotBeNull();
            restClient.Authenticator.Should().BeOfType<OAuth1Authenticator>();
        }

        [Test]
        public async Task Should_ComposeCorrectLoginUri_When_GetLoginLinkIsCalled()
        {
            // arrange
            var restClient = _factory.CreateClient();
            var restRequest = _factory.CreateRequest(null);
            var response = await restClient.Execute(restRequest).ConfigureAwait(false);
            response.RawBytes.Returns(_encoding.GetBytes("any content to pass response verification"));
            response.Content.Returns("oauth_token=token5&oauth_token_secret=secret");

            // act
            var uri = await _descendant.GetLoginLinkUri().ConfigureAwait(false);

            // assert
            uri.Should().Be("https://loginserviceendpoint/");

            _factory.Received().CreateClient();
            _factory.Received().CreateRequest("/LoginServiceEndpoint");
            
            restClient.Received().BaseUrl = new Uri("https://LoginServiceEndpoint");
            restRequest.Parameters.Received().AddOrUpdate(Arg.Is<Parameter>(x => x.Name == "oauth_token" && (string)x.Value == "token5"));
        }

        [Test]
        public async Task Should_IssueCorrectRequestForAccessToken_When_GetUserInfoIsCalled()
        {
            // arrange
            var restClient = _factory.CreateClient();
            var restRequest = _factory.CreateRequest(null);
            var response = await restClient.Execute(restRequest).ConfigureAwait(false);
            response.RawBytes.Returns(_encoding.GetBytes("any content to pass response verification"));
            response.Content.Returns("oauth_token=token5&oauth_token_secret=secret");

            // act
            await _descendant.GetUserInfo(new Dictionary<string, string>
            {
                {"oauth_token", "token1"},
                {"oauth_verifier", "verifier100"}
            }.ToLookup(y => y.Key, y => y.Value)).ConfigureAwait(false);

            // assert
            _factory.Received().CreateClient();
            _factory.Received().CreateRequest("/AccessTokenServiceEndpoint");

            restClient.Received().BaseUrl = new Uri("https://AccessTokenServiceEndpoint");
            restRequest.Received().Method = Method.POST;
            
            restClient.Authenticator.Should().NotBeNull();
            restClient.Authenticator.Should().BeOfType<OAuth1Authenticator>();
        }

        [Test]
        public async Task Should_IssueCorrectRequestForUserInfo_When_GetUserInfoIsCalled()
        {
            // arrange
            var restClient = _factory.CreateClient();
            var restRequest = _factory.CreateRequest(null);
            var response = await restClient.Execute(restRequest).ConfigureAwait(false);
            response.RawBytes.Returns(_encoding.GetBytes("any content to pass response verification"));
            response.Content.Returns("oauth_token=token&oauth_token_secret=secret", "abba");

            // act
            var info = await _descendant.GetUserInfo(new Dictionary<string, string>
            {
                {"oauth_token", "token1"},
                {"oauth_verifier", "verifier100"}
            }.ToLookup(y => y.Key, y => y.Value)).ConfigureAwait(false);

            // assert
            _factory.Received().CreateClient();
            _factory.Received().CreateRequest("/UserInfoServiceEndpoint");

            restClient.Received().BaseUrl = new Uri("https://UserInfoServiceEndpoint");

            restClient.Authenticator.Should().NotBeNull();
            restClient.Authenticator.Should().BeAssignableTo<OAuth1Authenticator>();

            info.Id.Should().Be("abba");
        }

        class OAuthClientDescendant : OAuthClient
        {
            public OAuthClientDescendant(IRequestFactory factory, IClientConfiguration configuration)
                : base(factory, configuration)
            {
            }
            
            protected override Endpoint RequestTokenServiceEndpoint
            {
                get
                {
                    return new Endpoint
                    {
                        BaseUri = "https://RequestTokenServiceEndpoint",
                        Resource = "/RequestTokenServiceEndpoint"
                    };
                }
            }

            protected override Endpoint LoginServiceEndpoint
            {
                get
                {
                    return new Endpoint
                    {
                        BaseUri = "https://LoginServiceEndpoint",
                        Resource = "/LoginServiceEndpoint"
                    };
                }
            }

            protected override Endpoint AccessTokenServiceEndpoint
            {
                get
                {
                    return new Endpoint
                    {
                        BaseUri = "https://AccessTokenServiceEndpoint",
                        Resource = "/AccessTokenServiceEndpoint"
                    };
                }
            }

            protected override Endpoint UserInfoServiceEndpoint
            {
                get
                {
                    return new Endpoint
                    {
                        BaseUri = "https://UserInfoServiceEndpoint",
                        Resource = "/UserInfoServiceEndpoint"
                    };
                }
            }

            public override string Name => "OAuthClientTest";

            protected override UserInfo ParseUserInfo(string content)
            {
                return Builder<UserInfo>.CreateNew()
                    .With(x => x.Id = content)
                    .Build();
            }
        }
    }

}
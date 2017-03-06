using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using FizzWare.NBuilder;

using NSubstitute;
using NUnit.Framework;

using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using RestSharp.Portable.OAuth2.Models;

using FluentAssertions;

namespace RestSharp.Portable.OAuth2.Tests.Client
{
    [TestFixture]
    public class OAuth2ClientTests
    {
        private OAuth2ClientDescendant _descendant;

        private IRequestFactory _factory;
        private IRestClient _restClient;
        private IRestRequest _restRequest;
        private IRestResponse _restResponse;

        private static readonly System.Text.Encoding _encoding = System.Text.Encoding.UTF8;

        [SetUp]
        public void SetUp()
        {
            _restRequest = Substitute.For<IRestRequest>();
            _restResponse = Substitute.For<IRestResponse>();

            _restResponse.StatusCode.Returns(HttpStatusCode.OK);
            _restResponse.RawBytes.Returns(_encoding.GetBytes("response"));

            _restClient = Substitute.For<IRestClient>();
            _restClient.Execute(_restRequest).Returns(Task.FromResult(_restResponse));

            _factory = Substitute.For<IRequestFactory>();
            _factory.CreateClient().Returns(_restClient);
            _factory.CreateRequest(null).ReturnsForAnyArgs(_restRequest);

            var configuration = Substitute.For<IClientConfiguration>();

            configuration.ClientId.Returns("client_id");
            configuration.ClientSecret.Returns("client_secret");
            configuration.RedirectUri.Returns("http://redirect-uri.net");
            configuration.Scope.Returns("scope");

            _descendant = new OAuth2ClientDescendant(_factory, configuration);
        }

        [Test]
        public void Should_ThrowUnexpectedResponse_When_CodeIsNotOk()
        {
            _restResponse.StatusCode.Returns(HttpStatusCode.InternalServerError);

            _descendant
                .Awaiting(x => x.GetUserInfo(new Dictionary<string, string>().ToLookup(y => y.Key, y => y.Value)))
                .ShouldThrow<UnexpectedResponseException>();
        }

        [Test]
        public void Should_ThrowUnexpectedResponse_When_ResponseIsEmpty()
        {
            _restResponse.StatusCode.Returns(HttpStatusCode.OK);
            _restResponse.RawBytes.Returns(_encoding.GetBytes(""));
            
            _descendant
                .Awaiting(x => x.GetUserInfo(new Dictionary<string, string>().ToLookup(y => y.Key, y => y.Value)))
                .ShouldThrow<UnexpectedResponseException>();
        }

        [Test]
        public async Task Should_ReturnCorrectAccessCodeRequestUri()
        {
            // arrange
            //restClient.BuildUrl(restRequest).Returns(new Uri("https://login-link.net/"));

            // act
            var uri = await _descendant.GetLoginLinkUri().ConfigureAwait(false);

            // assert
            uri.Should().Be("https://accesscodeserviceendpoint/");

            _factory.Received(1).CreateClient();
            _factory.Received(1).CreateRequest("/AccessCodeServiceEndpoint");

            _restClient.Received(1).BaseUrl = new Uri("https://AccessCodeServiceEndpoint");

            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "response_type" && (string)x.Value == "code"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "client_id" && (string)x.Value == "client_id"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "scope" && (string)x.Value == "scope"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "redirect_uri" && (string)x.Value == "http://redirect-uri.net"));
            _restRequest.Parameters.DidNotReceive().Add(Arg.Is<Parameter>(x => x.Name == "state"));
        }
        
        [Test]
        public void Should_ThrowException_WhenParametersForGetUserInfoContainError()
        {
            // arrange
            var parameters = new Dictionary<string, string> { { "error", "error2" } }.ToLookup(y => y.Key, y => y.Value);

            // act & assert
            _descendant
                .Awaiting(x => x.GetUserInfo(parameters))
                .ShouldThrow<UnexpectedResponseException>()
                .And.FieldName.Should().Be("error");
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ShouldNot_ThrowException_When_ParametersForGetUserInfoContainEmptyError(string error)
        {
            // arrange
            _restResponse.RawBytes.Returns(_encoding.GetBytes("any content to pass response verification"));
            _restResponse.Content.Returns("access_token=token");

            // act & assert
            _descendant
                .Awaiting(x => x.GetUserInfo(new Dictionary<string, string>
                {
                    {"error", error},
                    {"code", "code"}
                }.ToLookup(y => y.Key, y => y.Value)))
                .ShouldNotThrow();
        }

        [Test]
        public async Task Should_IssueCorrectRequestForAccessToken_When_GetUserInfoIsCalled()
        {
            // arrange
            _restResponse.RawBytes.Returns(_encoding.GetBytes("any content to pass response verification"));
            _restResponse.Content.Returns("access_token=token");

            // act
            await _descendant.GetUserInfo(new Dictionary<string, string> { { "code", "code" } }.ToLookup(y => y.Key, y => y.Value)).ConfigureAwait(false);

            // assert
            _factory.Received(1).CreateRequest("/AccessTokenServiceEndpoint");
            _restClient.Received(1).BaseUrl = new Uri("https://AccessTokenServiceEndpoint");
            _restRequest.Received(1).Method = Method.POST;
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "code" && (string)x.Value == "code"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "client_id" && (string)x.Value == "client_id"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "client_secret" && (string)x.Value == "client_secret"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "redirect_uri" && (string)x.Value == "http://redirect-uri.net"));
            _restRequest.Parameters.Received(1).Add(Arg.Is<Parameter>(x => x.Name == "grant_type" && (string)x.Value == "authorization_code"));
        }

        [Test]
        [TestCase("access_token=token")]
        [TestCase("{\"access_token\": \"token\"}")]
        public async Task Should_IssueCorrectRequestForUserInfo_When_GetUserInfoIsCalled(string response)
        {
            // arrange
            _restResponse.RawBytes.Returns(_encoding.GetBytes(response));
            _restResponse.Content.Returns(response);

            // act
            await _descendant.GetUserInfo(new Dictionary<string, string> { { "code", "code" } }.ToLookup(y => y.Key, y => y.Value)).ConfigureAwait(false);

            // assert
            _factory.Received(1).CreateRequest("/UserInfoServiceEndpoint");
            _restClient.Received(1).BaseUrl = new Uri("https://UserInfoServiceEndpoint");
            _restClient.Authenticator.Should().BeOfType<OAuth2UriQueryParameterAuthenticator>();
        }

        class OAuth2ClientDescendant : OAuth2Client
        {
            public OAuth2ClientDescendant(IRequestFactory factory, IClientConfiguration configuration) 
                : base(factory, configuration) 
            {
            }

            protected override Endpoint AccessCodeServiceEndpoint
            {
                get
                {
                    return new Endpoint
                    {
                        BaseUri = "https://AccessCodeServiceEndpoint",
                        Resource = "/AccessCodeServiceEndpoint"
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

            public override string Name
            {
                get { return "OAuth2ClientTest"; }
            }

            protected override UserInfo ParseUserInfo(IRestResponse response)
            {
                return Builder<UserInfo>.CreateNew()
                    .With(x => x.Id = response.Content)
                    .Build();
            }
        }
    }
}
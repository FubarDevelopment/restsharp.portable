using System.Linq;

using Xunit;

#pragma warning disable CS3016 // Arrays as attribut argument are not CLS compliant.
namespace RestSharp.Portable.Test
{
    /// <summary>
    /// Test cases taken from http://greenbytes.de/tech/tc/httpauth/
    /// </summary>
    public class AuthenticationHeaderValueTests
    {
        [Theory]
        [InlineData("Basic realm=\"foo\"", "realm=\"foo\"", "foo", "\"foo\"")]
        [InlineData("Basic\r\n realm=\"foo\"", "realm=\"foo\"", "foo", "\"foo\"")]
        [InlineData("Basic realm=foo", "realm=foo", "foo", "foo")]
        [InlineData(@"Basic realm=\f\o\o", "realm=\\f\\o\\o", "foo", "\\f\\o\\o")]
        [InlineData("Basic realm='foo'", "realm='foo'", "'foo'", "'foo'")]
        [InlineData("Basic realm=\"foo%20bar\"", "realm=\"foo%20bar\"", "foo%20bar", "\"foo%20bar\"")]
        [InlineData("Basic , realm=\"foo\"", "realm=\"foo\"", "foo", "\"foo\"")]
        [InlineData("Basic, realm=\"foo\"", "realm=\"foo\"", "foo", "\"foo\"")]
        [InlineData("Basic realm=\"\\f\\o\\o\"", "realm=\"\\f\\o\\o\"", "foo", "\"\\f\\o\\o\"")]
        [InlineData("Basic realm=\"\\\"foo\\\"\"", "realm=\"\\\"foo\\\"\"", "\"foo\"", "\"\\\"foo\\\"\"")]
        public void TestBasicWithRealm(string headerValue, string rawValue, string realmValue, string rawRealmValue)
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader(headerValue).ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal(rawValue, item.RawValue);
            Assert.Equal(realmValue, item.Values["realm"].Single());
            Assert.Equal(rawRealmValue, item.RawValues["realm"].Single());
        }

        [Fact]
        public void TestSimpleBasicNoRealm()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic").ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal(string.Empty, item.RawValue);
            Assert.Null(item.Values["realm"].SingleOrDefault());
            Assert.Null(item.RawValues["realm"].SingleOrDefault());
        }

        [Theory]
        [InlineData("BASIC REALM=\"foo\"", "BASIC", "REALM=\"foo\"", "foo", "\"foo\"")]
        public void TestMethodAndRealm(string headerValue, string method, string rawValue, string realmValue, string rawRealmValue)
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader(headerValue).ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal(method, item.Name);
            Assert.Equal(rawValue, item.RawValue);
            Assert.Equal(realmValue, item.Values["realm"].Single());
            Assert.Equal(rawRealmValue, item.RawValues["realm"].Single());
        }

        [Fact]
        public void TestSimpleBasicTwoRealms()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic realm=\"foo\", realm=\"bar\"").ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal("realm=\"foo\", realm=\"bar\"", item.RawValue);
            Assert.Collection(
                item.Values["realm"],
                value => Assert.Equal("foo", value),
                value => Assert.Equal("bar", value));
            Assert.Collection(
                item.RawValues["realm"],
                value => Assert.Equal("\"foo\"", value),
                value => Assert.Equal("\"bar\"", value));
        }

        [Fact]
        public void TestSimpleBasicWhiteSpaceRealm()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic realm = \"foo\"").ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal("realm = \"foo\"", item.RawValue);
            Assert.Collection(
                item.Values["realm"],
                value => Assert.Equal("foo", value));
            Assert.Collection(
                item.RawValues["realm"],
                value => Assert.Equal("\"foo\"", value));
        }

        [Fact]
        public void TestSimpleBasicNewParam1()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic realm=\"foo\", bar=\"xyz\",, a=b,,,c=d").ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal("realm=\"foo\", bar=\"xyz\",, a=b,,,c=d", item.RawValue);
            Assert.Collection(
                item.Values,
                param =>
                {
                    Assert.Equal("realm", param.Key);
                    Assert.Equal("foo", param.Single());
                },
                param =>
                {
                    Assert.Equal("bar", param.Key);
                    Assert.Equal("xyz", param.Single());
                },
                param =>
                {
                    Assert.Equal("a", param.Key);
                    Assert.Equal("b", param.Single());
                },
                param =>
                {
                    Assert.Equal("c", param.Key);
                    Assert.Equal("d", param.Single());
                });
            Assert.Collection(
                item.RawValues,
                param =>
                {
                    Assert.Equal("realm", param.Key);
                    Assert.Equal("\"foo\"", param.Single());
                },
                param =>
                {
                    Assert.Equal("bar", param.Key);
                    Assert.Equal("\"xyz\"", param.Single());
                },
                param =>
                {
                    Assert.Equal("a", param.Key);
                    Assert.Equal("b", param.Single());
                },
                param =>
                {
                    Assert.Equal("c", param.Key);
                    Assert.Equal("d", param.Single());
                });
        }

        [Fact]
        public void TestSimpleBasicNewParam2()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic bar=\"xyz\", realm=\"foo\"").ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal("bar=\"xyz\", realm=\"foo\"", item.RawValue);
            Assert.Collection(
                item.Values,
                param =>
                {
                    Assert.Equal("bar", param.Key);
                    Assert.Equal("xyz", param.Single());
                },
                param =>
                {
                    Assert.Equal("realm", param.Key);
                    Assert.Equal("foo", param.Single());
                });
            Assert.Collection(
                item.RawValues,
                param =>
                {
                    Assert.Equal("bar", param.Key);
                    Assert.Equal("\"xyz\"", param.Single());
                },
                param =>
                {
                    Assert.Equal("realm", param.Key);
                    Assert.Equal("\"foo\"", param.Single());
                });
        }

        [Fact]
        public void TestMultiBasicUnknown()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic realm=\"basic\", Newauth realm=\"newauth\"").ToList();
            Assert.Collection(
                items,
                item =>
                {
                    Assert.Equal("Basic", item.Name);
                    Assert.Equal("realm=\"basic\"", item.RawValue);
                    Assert.Equal("basic", item.Values["realm"].Single());
                    Assert.Equal("\"basic\"", item.RawValues["realm"].Single());
                },
                item =>
                {
                    Assert.Equal("Newauth", item.Name);
                    Assert.Equal("realm=\"newauth\"", item.RawValue);
                    Assert.Equal("newauth", item.Values["realm"].Single());
                    Assert.Equal("\"newauth\"", item.RawValues["realm"].Single());
                });
        }

        [Fact]
        public void TestMultiBasicEmpty()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader(",Basic realm=\"basic\"").ToList();
            Assert.Collection(
                items,
                item =>
                {
                    Assert.Equal("Basic", item.Name);
                    Assert.Equal("realm=\"basic\"", item.RawValue);
                    Assert.Equal("basic", item.Values["realm"].Single());
                    Assert.Equal("\"basic\"", item.RawValues["realm"].Single());
                });
        }

        [Fact]
        public void TestMultiBasicQuotedStrings()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Newauth realm=\"apps\", type=1, title=\"Login to \\\"apps\\\"\", Basic realm = \"simple\"").ToList();
            Assert.Collection(
                items,
                item =>
                {
                    Assert.Equal("Newauth", item.Name);
                    Assert.Equal("realm=\"apps\", type=1, title=\"Login to \\\"apps\\\"\"", item.RawValue);
                    Assert.Collection(
                        item.Values,
                        value =>
                        {
                            Assert.Equal("realm", value.Key);
                            Assert.Equal("apps", value.Single());
                        },
                        value =>
                        {
                            Assert.Equal("type", value.Key);
                            Assert.Equal("1", value.Single());
                        },
                        value =>
                        {
                            Assert.Equal("title", value.Key);
                            Assert.Equal("Login to \"apps\"", value.Single());
                        });
                    Assert.Collection(
                        item.RawValues,
                        value =>
                        {
                            Assert.Equal("realm", value.Key);
                            Assert.Equal("\"apps\"", value.Single());
                        },
                        value =>
                        {
                            Assert.Equal("type", value.Key);
                            Assert.Equal("1", value.Single());
                        },
                        value =>
                        {
                            Assert.Equal("title", value.Key);
                            Assert.Equal("\"Login to \\\"apps\\\"\"", value.Single());
                        });
                },
                item =>
                {
                    Assert.Equal("Basic", item.Name);
                    Assert.Equal("realm = \"simple\"", item.RawValue);
                    Assert.Equal("simple", item.Values["realm"].Single());
                    Assert.Equal("\"simple\"", item.RawValues["realm"].Single());
                });
        }

        [Fact]
        public void TestMissingQuote()
        {
            var items = AuthHeaderUtilities.ParseAuthenticationHeader("Basic realm=\"basic").ToList();
            Assert.Equal(1, items.Count);

            var item = items[0];
            Assert.Equal("Basic", item.Name);
            Assert.Equal("realm=\"basic", item.RawValue);
            Assert.Equal("basic", item.Values["realm"].Single());
            Assert.Equal("\"basic", item.RawValues["realm"].Single());
        }
    }
}

# Changes

## 4.0.0

### New features

* Support for `netstandard1.0` and above
* Support for `HMAC-SHA256`

### Breaking changes

* OAuth 1.0 Package renamed to FubarCoder.RestSharp.Portable.OAuth1
* The `AddHandler`, `RemoveHandler`, `ClearHandlers` replaced by `ContentHandlers` dictionary
* The `AddEncoding`, `RemoveEncoding`, `ClearEncodings` replaced by `EncodingHandlers` dictionary
* `IRequestProxy` removed. We're now using `IWebProxy`. The `Proxy` property doesn't exist any more on unsupported platforms
* The same parameter is allowed multiple times. You can use the `AddOrUpdate*` functions when you only want to have one parameter with the same name
* `GetOrPost` and `QueryString` parameters are different - even when issuing a GET request

### Probably breaking Changes

* `GetOrPost` parameters are sent as `x-www-form-urlencoded` when the request method is **not** `GET`

## 3.3.0

This is a backport of changes from the 4.0 branch.

### Possible breaking changes

* Fix for [issue #71](https://github.com/FubarDevelopment/restsharp.portable/issues/71): Support to send multiple parameters with the same name. You can use `AddOrUpdate` when every named parameter must occur only once. There is no `ParameterComparer` any more. The `Parameters` and `DefaultParameters` properties are now a `IPropertyCollection` which is derived from `ICollection<Parameter>`, and **not** an `IList<Parameter>` any more!
* Fix for [issue #69](https://github.com/FubarDevelopment/restsharp.portable/issues/69): `GetOrPost` parameters are sent as `x-www-form-urlencoded` when the request method is **not** `GET`.

## 3.2.0

* New `Content` property for `IRestResponse`
* Updated Android targets to 4.0.3 (due to 2.3.3 SDK not being installable)

## 3.1.3

* Fix for [issue #57](https://github.com/FubarDevelopment/restsharp.portable/issues/57) (thanks to zevsst)
* Fix for [issue #55](https://github.com/FubarDevelopment/restsharp.portable/issues/55)

## 3.1.0

This is the final release for version 3. Be aware that version 3.1 contains many **breaking changes**:

* The `IRestRequest.Credentials` property moved to `IRestClient.Credentials`
* Credentials for authenticators are specified using the `IRestClient.Credentials` property
* New core library that contains all interfaces and other generic stuff
* New interface for proxies
* New interfaces that are an abstraction of the HttpClient and its request/response messages
* Uses now a RestSharp project compatible Method enumeration for HTTP requests
* Support for WebRequest as back-end for RestSharp.Portable

## 3.0.0 beta 12

* A special URL encoding is required for query parameters used to calculate
  the OAuth 1.0 signature - fixes sending a URL as Twitter status

## 3.0.0 beta 11

* Async locking fixes (provided by [evnik](https://github.com/evnik))
* OAuth 1.0 fixes (required for Twitter)
  * UTF-8 encoding for characters like `\u2764\uFE0F` (❤️)
  * Use [EscapeUriString](https://msdn.microsoft.com/de-de/library/system.uri.escapeuristring.aspx) compatible URL encoding for query parameters

## 3.0.0 beta 09

Fix endless loop for OAuth2 and failed requests when a refresh token is available.

## 3.0.0 beta 07

Remember the OAuth2 refresh token when calling the `GetCurrentToken` function with a set
refresh token. This should fix the problem with exiring access tokens.

## 3.0.0 beta 06

* Revert to Portable.BouncyCastle-Signed to be able to use a release version
* Use the JetBrains.Annotations assembly to fix problems when creating CoreCLR assemblies
  that want to use JetBrains.Annotations too

## 3.0.0 beta 04

* Don't discard GetOrPost parameters when using PUT or PATCH

## 3.0.0 beta 03

* Fix for [issue #42](https://github.com/FubarDevelopment/restsharp.portable/issues/42)

## 3.0.0 beta 02

* Support more headers when using `HttpWebRequest` instead of `HttpClient`
* New `UserAgent` property for `IRestClient`
* Activated automatic decompression for `HttpClient` and `HttpWebRequest`

## 3.0.0 beta 01

* Refactoring to reduce source code duplication by adding a `RestClientBase` class
* Allow customization of `HttpWebRequest` creation to allow the usage of client certificates
* Support for RSA-SHA1 for platforms with full .NET Framework support
* New OAuth 1.0 test that uses [oauthbin.com](http://oauthbin.com)

## 3.0.0 alpha 10

* Fixed HttpWebRequest usage for Windows Store apps

## 3.0.0 alpha 9

* Added PCL for OAuth/OAuth2 with profile 259 (better for platforms that don't target .NET 4.0 or SL5)
* Make all assemblies containing WebRequest implementations signed

## 3.0.0 alpha 5-8

* Added RestSharp implementation using WebRequest instead of HttpClient
* Added PCL optimizations (for .NET 4.5 and up)

## 3.0.0 alpha 4

* Moved all interfaces and other generic stuff into a separate core library
  that doesn't have any dependencies
* New interfaces that are an abstraction of the HttpClient and its 
  request/response messages
* New interface for proxies
* Uses now a RestSharp project compatible Method enumeration for HTTP requests and
  all constructors taking a HttpMethod are flagged as obsolete
* The IAuthenticator, ISerializer, IEncoding, and IDeserializer interfaces were
  moved to the RestSharp.Portable namespace

## 3.0.0 alpha 3

* Revamped authenticator interfaces
  * Provide a way to process the `Www-Authenticate` header
  * Make HTTP Basic/Digest authenticators work with `Proxy-Authenticate` header
  * Credentials property moved from `IRestRequest` to `IRestClient`
  * The NTLM authenticator is not needed anymore, because the the credentials from 
    the `IRestRequest` are automatically used in the `HttpClientHandler` which handles
    the Basic/Digest/NTLM authentication automatically
  * All authenticators should query the credentials passed to the authenticator
  * New `AuthenticationChallengeHandler` which selects one of the registered
    authenticators in response to a `Www-Authenticate` or `Proxy-Authenticate` challenge.
* New Gitter OAuth 2.0 client

## 2.4.5

* Async locking fixes (provided by [evnik](https://github.com/evnik))

## 2.4.4

* Bugfix for issue [#29](https://github.com/FubarDevelopment/restsharp.portable/issues/29).

## 2.4.3

* Bugfix for issue [#23](https://github.com/FubarDevelopment/restsharp.portable/issues/23).
  Thanks to [GeirGrusom](https://github.com/GeirGrusom)

## 2.4.2

* Bugfix for issue [#25](https://github.com/FubarDevelopment/restsharp.portable/issues/25).
  We're using asynchronous locking now.

## 2.4.1

* Bugfix for issue [#24](https://github.com/FubarDevelopment/restsharp.portable/issues/24)
  which should allow using both OAuth 1.0 and 2.0 in Android apps.

## 2.4.0

* New `Timeout` property to fix issue [#13](https://github.com/FubarDevelopment/restsharp.portable/issues/13) with [CancellationTokenSource.CancelAfter](https://msdn.microsoft.com/de-de/library/hh194678%28v=vs.110%29.aspx)

## 2.3.2

* Fixes concurrent requests on the same RestClient (fixes [#19](https://github.com/FubarDevelopment/restsharp.portable/issues/19), 
  bug introduced with [#11](https://github.com/FubarDevelopment/restsharp.portable/issues/11))

## 2.3.1

* Fixes issue [#18](https://github.com/FubarDevelopment/restsharp.portable/issues/18)
  * All DateTime(Offset) HTTP header values are encoded as described in RFC 1123 after conversion to UTC/GMT
* All data is converted to a string using the en-US culture (might be a breaking change)

## 2.3.0

* Fixes issue [#17](https://github.com/FubarDevelopment/restsharp.portable/issues/17)
  * New IsSuccess property for IRestResponse

## 2.2.0

* Fixes issue [#15](https://github.com/FubarDevelopment/restsharp.portable/issues/15)
  * Add the ability to provide a custom timestamp provider
* Fixes issue [#16](https://github.com/FubarDevelopment/restsharp.portable/issues/16)
  * Remove superfluous "?" when using URL segment parameters in the query string

## 2.1.1

* Fixed broken SL5 support (thanks to P2SH)

## 2.1.0

* Fixes issue [#11](https://github.com/FubarDevelopment/restsharp.portable/issues/11)
  * IRestClient now derives from IDisposable
  * HttpClient is kept alive until the RestClient gets disposed
  * Default HTTP header parameters are set for the
    HttpClient
* Fixes issue [#12](https://github.com/FubarDevelopment/restsharp.portable/issues/12)
  * Workaround for the 32k limit of EscapeDataString
  * Custom class for URL encoding that's
    used as fall-back, when the user wants to use 
    EscapeDataString with a byte array (which isn't supported).
* Avoid rebuilding the Basic Authentication header for each request 

## 2.0.3

* Fixed NuGet dependency for the OAuth 1.0 package
* Fixed some problems found by FxCop

## 2.0.2

* Fixed NuGet package for Xamarin.iOS (upload using nuget instead of NPE)

## 2.0.1

* Fixed Microsoft.Bcl and Microsoft.Bcl.Build dependencies
* Assemblies are now CLSCompliant (except PCL and SL5, which don't support this attribute)

## 2.0.0

* Removed all deprecated methods
* Starting from this version, I'll use [Semantic Versioning 2.0.0](http://semver.org/)
* Optimizing NuGet dependencies for several platforms
* Clear Accept HTTP header parameter for the SL5 platform for GET requests ([Issue #9](https://github.com/FubarDevelopment/restsharp.portable/issues/9))
* Add Deflate encoding

## 1.9.1

* OAuth2AuthorizationRequestHeaderAuthenticator should only check for Authorization
  header parameter
* Better handling of refresh tokens in the OAuth2AuthorizationRequestHeaderAuthenticator

## 1.9.0

* Increased compatibility with the original RestSharp project
	* BuildUri instead of BuildUrl (deprecated)
	* Added AddJsonBody, AddXmlBody, AddQueryParameter, AddObject
* Graceful handling of duplicate parameters
  (might be a breaking change)
* Dispose HttpClient, HttpRequestMessage and the HttpResponseMessage

## 1.8.5

* BuildUrl adds a "/" between a base URL and resource 
  if neither of them is empty and the "/" is missing
* Fix BOM for XmlDataContractSerializer
* Better support for OAuth2 refresh tokens by supporting
  a HTTP 401 by the OAuth2 authenticator (when a refresh 
  token was set)

## 1.8.4

* Support for parameters in IRestClient.BaseUrl
* Signed OAuth1/OAuth2 assemblies
* Increased compatibility for empty IRestClient.BaseUrl

## 1.8.3

* Workaround for NuGet pack bug

## 1.8.2

* Encodings for parameters (get/post/url/query)

## 1.8.1

* Async. authenticators
* New OAuth2 package

## 1.8.0

* Cancellable requests
* New OAuth 1.0 package

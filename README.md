# Portable RestSharp #

This is some kind of a RestSharp port to PCL.

# [BSD 2-Clause License](LICENSE.md) #

# News #

* Version 2.3.2 is available now!

# Changes #

## 2.3.2 ##

* Fixes concurrent requests on the same RestClient (fixes [#19](https://github.com/FubarDevelopment/restsharp.portable/issues/19), 
  bug introduced with [#11](https://github.com/FubarDevelopment/restsharp.portable/issues/11))

## 2.3.1 ##

* Fixes issue [#18](https://github.com/FubarDevelopment/restsharp.portable/issues/18)
  * All DateTime(Offset) HTTP header values are encoded as described in RFC 1123 after conversion to UTC/GMT
* All data is converted to a string using the en-US culture (might be a breaking change)

## 2.3.0 ##

* Fixes issue [#17](https://github.com/FubarDevelopment/restsharp.portable/issues/17)
  * New IsSuccess property for IRestResponse

## 2.2.0 ##

* Fixes issue [#15](https://github.com/FubarDevelopment/restsharp.portable/issues/15)
  * Add the ability to provide a custom timestamp provider
* Fixes issue [#16](https://github.com/FubarDevelopment/restsharp.portable/issues/16)
  * Remove superfluous "?" when using URL segment parameters in the query string

## 2.1.1 ##

* Fixed broken SL5 support (thanks to P2SH)

## 2.1.0 ##

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

## 2.0.3 ##

* Fixed NuGet dependency for the OAuth 1.0 package
* Fixed some problems found by FxCop

## 2.0.2 ##

* Fixed NuGet package for Xamarin.iOS (upload using nuget instead of NPE)

## 2.0.1 ##

* Fixed Microsoft.Bcl and Microsoft.Bcl.Build dependencies
* Assemblies are now CLSCompliant (except PCL and SL5, which don't support this attribute)

## 2.0.0 ##

* Removed all deprecated methods
* Starting from this version, I'll use [Semantic Versioning 2.0.0](http://semver.org/)
* Optimizing NuGet dependencies for several platforms
* Clear Accept HTTP header parameter for the SL5 platform for GET requests ([Issue #9](https://github.com/FubarDevelopment/restsharp.portable/issues/9))
* Add Deflate encoding

## 1.9.1 ##

* OAuth2AuthorizationRequestHeaderAuthenticator should only check for Authorization
  header parameter
* Better handling of refresh tokens in the OAuth2AuthorizationRequestHeaderAuthenticator

## 1.9.0 ##

* Increased compatibility with the original RestSharp project
	* BuildUri instead of BuildUrl (deprecated)
	* Added AddJsonBody, AddXmlBody, AddQueryParameter, AddObject
* Graceful handling of duplicate parameters
  (might be a breaking change)
* Dispose HttpClient, HttpRequestMessage and the HttpResponseMessage

## 1.8.5 ##

* BuildUrl adds a "/" between a base URL and resource 
  if neither of them is empty and the "/" is missing
* Fix BOM for XmlDataContractSerializer
* Better support for OAuth2 refresh tokens by supporting
  a HTTP 401 by the OAuth2 authenticator (when a refresh 
  token was set)

## 1.8.4 ##

* Support for parameters in IRestClient.BaseUrl
* Signed OAuth1/OAuth2 assemblies
* Increased compatibility for empty IRestClient.BaseUrl

## 1.8.3 ##

* Workaround for NuGet pack bug

## 1.8.2 ##

* Encodings for parameters (get/post/url/query)

## 1.8.1 ##

* Async. authenticators
* New OAuth2 package

## 1.8.0 ##

* Cancellable requests
* New OAuth 1.0 package

# Supported platforms

* .NET Framework 4
* .NET for Windows Store apps
* .NET Native
* Windows Phone 8 and 8.1
* Silverlight 5
* Portable Class Libraries

# Small example

The following is an example to get the ticker from the bitstamp.net website.

## The result class
```csharp
public class TickerResult
{
	public decimal Last { get; set; }
	public decimal High { get; set; }
	public decimal Low { get; set; }
	public decimal Volume { get; set; }
	public decimal Bid { get; set; }
	public decimal Ask { get; set; }
}
```

We use the class with:

```csharp
using (var client = new RestClient(new Uri("https://www.bitstamp.net/api/")))
{
    var request = new RestRequest("ticker", HttpMethod.Get);
    var result = await client.Execute<TickerResult>(request);
}
```

# Community Support #

The support for community projects can be found in my [subreddit /r/FubarDev](http://www.reddit.com/r/FubarDev/).

# Professional Support #

You can get professional support here: [Fubar Development Junker](https://www.fubar-dev.de)

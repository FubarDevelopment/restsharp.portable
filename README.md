# Portable RestSharp #

This is some kind of a RestSharp port to PCL.

# [BSD 2-Clause License](BSD 2-Clause License) #

# News #

* Version 1.8.4 is available now!

# Changes #

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
var client = new RestClient(new Uri("https://www.bitstamp.net/api/"));
var request = new RestRequest("ticker", HttpMethod.Get);
var result = await client.Execute<TickerResult>(request);
```

# Profressional Support #

You can get professional support here: [Fubar Development Junker](https://www.fubar-dev.de)
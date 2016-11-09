# Portable RestSharp

[![Build status](https://build.fubar-dev.de/app/rest/builds/buildType:%28id:RestSharpPortable_40Preview%29/statusIcon)](https://build.fubar-dev.com/project.html?projectId=RestSharpPortable)

[![Join the chat at https://gitter.im/FubarDevelopment/restsharp.portable](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/FubarDevelopment/restsharp.portable?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This is some kind of a RestSharp port as PCL and for .NET Core.

# License

This project is licensed using the [BSD 2-Clause License](LICENSE.md)

# NuGet packages

| Description				                | Badge |
|-------------------------------------------|-------|
| Core library				                | [![FubarCoder.RestSharp.Portable.Core](https://img.shields.io/nuget/v/FubarCoder.RestSharp.Portable.Core.svg)](https://www.nuget.org/packages/FubarCoder.RestSharp.Portable.Core) |
| Request engine using `HttpWebRequest`	    | [![FubarCoder.RestSharp.Portable.WebRequest](https://img.shields.io/nuget/v/FubarCoder.RestSharp.Portable.WebRequest.svg)](https://www.nuget.org/packages/FubarCoder.RestSharp.Portable.WebRequest) |
| Request engine using `HttpClient`		    | [![FubarCoder.RestSharp.Portable.HttpClient](https://img.shields.io/nuget/v/FubarCoder.RestSharp.Portable.HttpClient.svg)](https://www.nuget.org/packages/FubarCoder.RestSharp.Portable.HttpClient) |
| OAuth 1.0(a) authentication support	    | [![FubarCoder.RestSharp.Portable.OAuth1](https://img.shields.io/nuget/v/FubarCoder.RestSharp.Portable.OAuth1.svg)](https://www.nuget.org/packages/FubarCoder.RestSharp.Portable.OAuth1) |
| OAuth 2.0 authentication support	        | [![FubarCoder.RestSharp.Portable.OAuth2](https://img.shields.io/nuget/v/FubarCoder.RestSharp.Portable.OAuth2.svg)](https://www.nuget.org/packages/FubarCoder.RestSharp.Portable.OAuth2) |
| Content encoding support (GZip/Deflate)   | [![FubarCoder.RestSharp.Portable.Encodings](https://img.shields.io/nuget/v/FubarCoder.RestSharp.Portable.Encodings.svg)](https://www.nuget.org/packages/FubarCoder.RestSharp.Portable.Encodings) |

# Request engines

RestSharp.Portable can use either `HttpWebRequest` or `HttpClient` as request engine.

## `HttpWebRequest` request engine

Advantage:

- Mature

Disadvantage:

- Not configurable

## `HttpClient` request engine

Advantage:

- Configurable (maybe better performance on iOS/Adroid through [ModernHttpClient](https://github.com/paulcbetts/ModernHttpClient))

Disadvantage:

- Behaves differently on different platforms

# [Changes](Changes.md)

# Supported platforms

* .NET Core
* .NET Framework 4
* .NET for Windows Store apps
* .NET Native
* Windows Phone 8 and 8.1
* Silverlight 5
* Portable Class Libraries
* Xamarin Android
* Xamarin MonoTouch / iOS

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
    var request = new RestRequest("ticker", Method.GET);
    var result = await client.Execute<TickerResult>(request);
}
```

# Contributors

* [Henk Mollema](https://github.com/henkmollema)
* [James Humphries](https://github.com/Yantrio)
* [Jonathan Channon](https://github.com/jchannon)
* [Eugene Berdnikov](https://github.com/evnik)
* [Henning Moe](https://github.com/GeirGrusom)
* [P2SH](https://github.com/P2SH)
* [Ingvar Stepanyan](https://github.com/RReverser)

# Community Support

The support for community projects can be found in my [subreddit /r/FubarDev](http://www.reddit.com/r/FubarDev/).

# Professional Support

You can get professional support here: [Fubar Development Junker](https://www.fubar-dev.de)

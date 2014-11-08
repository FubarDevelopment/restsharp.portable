# Portable RestSharp #

This is some kind of a RestSharp port to PCL.

# [BSD 2-Clause License](BSD 2-Clause License) #

# News #

* A port of an OAuth 2.0 library will be available soon!
* Version 1.8.0 is available now!

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
```
#!csharp
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

```
#!csharp

var client = new RestClient(new Uri("https://www.bitstamp.net/api/"));
var request = new RestRequest("ticker", HttpMethod.Get);
var result = await client.Execute<TickerResult>(request);
```

# Profressional Support #

You can get professional support here: [Fubar Development Junker](https://www.fubar-dev.de)
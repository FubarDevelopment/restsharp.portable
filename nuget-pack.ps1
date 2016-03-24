[OutputType([void])]
param(
	[Parameter()]
	$version = "3.1.3",
	[Parameter()]
	$config = "Release"
)

remove-item *.nupkg

& nuget pack .\RestSharp.Portable.nuspec -Properties "clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.Core.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.HttpClient.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.WebRequest.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.Encodings.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth2.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version

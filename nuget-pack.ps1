[OutputType([void])]
param(
	[Parameter()]
	$version = "1.8.4",
	[Parameter()]
	$config = "Release",
	[Parameter()]
	$oauth2version = "0.8.34.4"
)

& nuget pack .\RestSharp.Portable\RestSharp.Portable.csproj -Properties Configuration=$config -Version $version
& nuget pack .\RestSharp.Portable.Encodings\RestSharp.Portable.Encodings.csproj -Properties Configuration=$config -Version $version
& nuget pack .\RestSharp.Portable.OAuth\RestSharp.Portable.OAuth.csproj -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth2\RestSharp.Portable.OAuth2.csproj -Properties "Configuration=$config;clientversion=$version" -Version $oauth2version

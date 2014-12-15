[OutputType([void])]
param(
	[Parameter()]
	$version = "1.9.2",
	[Parameter()]
	$config = "Release",
	[Parameter()]
	$oauth2version = "0.8.34.8"
)

& nuget pack .\RestSharp.Portable.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.Encodings.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth\RestSharp.Portable.OAuth.csproj -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth2\RestSharp.Portable.OAuth2.csproj -Properties "Configuration=$config;clientversion=$version" -Version $oauth2version

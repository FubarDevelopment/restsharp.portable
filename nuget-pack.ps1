[OutputType([void])]
param(
	[Parameter()]
	$version = "2.3.1",
	[Parameter()]
	$config = "Release"
)

& nuget pack .\RestSharp.Portable.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.Encodings.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version
& nuget pack .\RestSharp.Portable.OAuth2.nuspec -Properties "Configuration=$config;clientversion=$version" -Version $version

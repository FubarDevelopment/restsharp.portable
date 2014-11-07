[OutputType([void])]
param(
	[Parameter(Mandatory = $true)]
	$version,
	[Parameter()]
	$config = "Release"
)

& nuget pack .\RestSharp.Portable\RestSharp.Portable.csproj -Properties Configuration=$config -Version $version
& nuget pack .\RestSharp.Portable.Encodings\RestSharp.Portable.Encodings.csproj -Properties Configuration=$config -Version $version
& nuget pack .\RestSharp.Portable.OAuth\RestSharp.Portable.OAuth.csproj -Properties Configuration=$config -Version $version

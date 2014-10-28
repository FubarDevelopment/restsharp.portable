param(
	[Parameter(Mandatory = $true)]
	$version
)

& nuget pack .\RestSharp.Portable\RestSharp.Portable.csproj -Properties Configuration=Release -Version $version
& nuget pack .\RestSharp.Portable.Encodings\RestSharp.Portable.Encodings.csproj -Properties Configuration=Release -Version $version

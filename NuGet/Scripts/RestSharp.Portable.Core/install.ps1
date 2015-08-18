param($installPath, $toolsPath, $package, $project)

$getTargetFrameworkMoniker = [Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim].GetMethod("get_TargetFrameworkMoniker")
$targetFrameworkVersion = (New-Object System.Runtime.Versioning.FrameworkName $getTargetFrameworkMoniker.Invoke($project.Object, $null)).Version
$ver45 = New-Object System.Version 4,5

$fwPath = "portable-net40+sl50+win+wpa81+wp80+MonoAndroid10+MonoTouch10+Xamarin.iOS10"
if ($targetFrameworkVersion -ge $ver45) {
	$fwPath = "portable-net45+win+wpa81+wp80+MonoAndroid10+MonoTouch10+Xamarin.iOS10"
}

$assemblyPath = [System.IO.Path]::Combine($toolsPath, "..\..\lib\$fwPath\RestSharp.Portable.Core.dll")
$obj = $project.Object
$getRefsMethod = [Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim].GetMethod("get_References")
$refs = $getRefsMethod.Invoke($obj, $null)
$refs.Add($assemblyPath)

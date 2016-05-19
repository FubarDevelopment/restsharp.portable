param($installPath, $toolsPath, $package, $project)

$getTargetFrameworkMoniker = [Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim].GetMethod("get_TargetFrameworkMoniker")
$targetFrameworkVersion = (New-Object System.Runtime.Versioning.FrameworkName $getTargetFrameworkMoniker.Invoke($project.Object, $null)).Version
$ver45 = New-Object System.Version 4,5

$fwPath = "net40"
if ($targetFrameworkVersion -ge $ver45) {
	$fwPath = "net45"
}

$assemblyPath = [System.IO.Path]::Combine($toolsPath, "..\..\lib\$fwPath\FubarCoder.RestSharp.Portable.Core.dll")
$obj = $project.Object
$getRefsMethod = [Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim].GetMethod("get_References")
$refs = $getRefsMethod.Invoke($obj, $null)
$refs.Add($assemblyPath)

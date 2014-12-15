param($installPath, $toolsPath, $package, $project)

$assemblyPath = [System.IO.Path]::Combine($toolsPath, "..\..\lib\portable-net45+win+wpa81+wp80+MonoAndroid10+MonoTouch10\RestSharp.Portable.Encodings.dll")
$obj = $project.Object
$getRefsMethod = [Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim].GetMethod("get_References")
$refs = $getRefsMethod.Invoke($obj, $null)
$refs.Add($assemblyPath)

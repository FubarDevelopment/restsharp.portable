param($installPath, $toolsPath, $package, $project)

$assemblyPath = [System.IO.Path]::Combine($toolsPath, "..\lib\portable-net4+sl5+wp8+win8\RestSharp.Portable.dll")
$obj = $project.Object
$getRefsMethod = [Microsoft.VisualStudio.Project.VisualC.VCProjectEngine.VCProjectShim].GetMethod("get_References")
$refs = $getRefsMethod.Invoke($obj, $null)
$refs.Add($assemblyPath)

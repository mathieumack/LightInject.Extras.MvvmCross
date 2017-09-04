write-host "**************************" -foreground "Cyan"
write-host "*   Packaging to nuget   *" -foreground "Cyan"
write-host "**************************" -foreground "Cyan"

#$location  = "C:\Sources\NoSqlRepositories";
$location  = $env:APPVEYOR_BUILD_FOLDER

$locationNuspec = $location + "\nuspec"
$locationNuspec
	
Set-Location -Path $locationNuspec

$strPath = $location + '\LightInject.Extras.MvvmCross.Pcl\bin\Release\LightInject.Extras.MvvmCross.Pcl.dll'

$VersionInfos = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($strPath)
$ProductVersion = $VersionInfos.ProductVersion
write-host "Product version : " $ProductVersion -foreground "Green"

write-host "Packaging to nuget..." -foreground "Magenta"

write-host "Update nuspec versions" -foreground "Green"

write-host "Update nuspec versions LightInject.Extras.MvvmCross.Pcl.nuspec" -foreground "DarkGray"
$nuSpecFile =  $locationNuspec + '\LightInject.Extras.MvvmCross.Pcl.nuspec'
(Get-Content $nuSpecFile) | 
Foreach-Object {$_ -replace "{BuildNumberVersion}", "$ProductVersion" } | 
Set-Content $nuSpecFile

write-host "Generate nuget packages" -foreground "Green"

write-host "Generate nuget package for LightInject.Extras.MvvmCross.Pcl.nuspec" -foreground "DarkGray"
.\NuGet.exe pack LightInject.Extras.MvvmCross.Pcl.nuspec

$apiKey = $env:NuGetApiKey
	
write-host "Publish nuget packages" -foreground "Green"

write-host LightInject.Extras.MvvmCross.Pcl.$ProductVersion.nupkg -foreground "DarkGray"
.\NuGet push LightInject.Extras.MvvmCross.Pcl.$ProductVersion.nupkg -Source https://www.nuget.org/api/v2/package -ApiKey $apiKey
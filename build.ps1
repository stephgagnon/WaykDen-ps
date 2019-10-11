Remove-Item -Path '.\package' -Recurse -Force -ErrorAction SilentlyContinue

$version = dotnet --version

$pathCore = '.\package\WaykDen\Core'
$pathDesktop = '.\package\WaykDen\Desktop'
if($version[0] -eq '2'){
	$pathCore = '..\package\WaykDen\Core'
	$pathDesktop = '..\package\WaykDen\Desktop'
}

& dotnet publish '.\src\WaykDen.sln' -f netcoreapp2.2 -c Release -o "$pathCore"

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
	& dotnet publish '.\src\WaykDen.sln' -f net472 -c Release -o "$pathDesktop"
}

Copy-Item '.\src\WaykDen.psd1' -Destination '.\package\WaykDen'
Remove-Item -Path '.\WaykDen\package' -Recurse -Force -ErrorAction SilentlyContinue

& dotnet publish '.\src\WaykDen.sln' -f netcoreapp2.2 -c Release -o '..\WaykDen\package\WaykDen\Core'

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
	New-Item -path '.\WaykDen\package\WaykDen\Desktop' -ItemType Directory -Force
	& dotnet publish '.\src\WaykDen.sln' -f net472 -c Release -o '..\WaykDen\package\WaykDen\Desktop'
}

Copy-Item '.\src\WaykDen.psd1' -Destination '.\WaykDen\package\WaykDen'
Remove-Item -Path '.\WaykDen\package' -Recurse -Force -ErrorAction SilentlyContinue

& dotnet publish -f netcoreapp2.2 -c Release -o '.\WaykDen\package\WaykDen\Core' '.\src\WaykDen.sln'

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
	& dotnet publish -f net472 -c Release -o '.\WaykDen\package\WaykDen\Desktop' '.\src\WaykDen.sln'
}

Copy-Item '.\src\WaykDen.psd1' -Destination '.\WaykDen\package\WaykDen'

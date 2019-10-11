
$ModuleName = 'WaykDen'
Remove-Item -Path .\$ModuleName\package -Recurse -Force -ErrorAction SilentlyContinue

& dotnet publish -f netcoreapp2.2 -c Release -o ./$ModuleName/package/$ModuleName/Core ./src/WaykDen.sln

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
	& dotnet publish -f net472 -c Release -o ./$ModuleName/package/$ModuleName/Desktop ./src/WaykDen.sln
}

Copy-Item .\src\$ModuleName.psd1 -Destination .\$ModuleName\package\$ModuleName

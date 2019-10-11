
$ModuleName = 'WaykDen'
Remove-Item -Path .\$ModuleName\package -Recurse -Force -ErrorAction SilentlyContinue

& dotnet publish -f netcoreapp2.2 -c Release -o ./$ModuleName/package/Core ./src/WaykDen.sln

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
	& dotnet publish -f net472 -c Release -o ./$ModuleName/package/Desktop ./src/WaykDen.sln
}


$ModuleName = 'WaykDen'
Remove-Item -Path .\package -Recurse -Force 

& dotnet publish -f netcoreapp2.2 -c Release -o package/$ModuleName/Core

if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
	& dotnet publish -f net472 -c Release -o package/$ModuleName/Desktop
}

Copy-Item .\$ModuleName.psd1 -Destination .\package\$ModuleName

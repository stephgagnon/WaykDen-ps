
$module = 'WaykDen'
Push-Location $PSScriptRoot

Remove-Item -Path .\package -Recurse -Force -ErrorAction SilentlyContinue
& dotnet publish "$PSScriptRoot\$module\src" -f netstandard2.0 -c Release -o "$PSScriptRoot\package\$module\bin"

Copy-Item "$PSScriptRoot\$module\Private" -Destination "$PSScriptRoot\package\$module" -Recurse -Force
Copy-Item "$PSScriptRoot\$module\Public" -Destination "$PSScriptRoot\package\$module" -Recurse -Force

Copy-Item "$PSScriptRoot\$module\$module.psd1" -Destination "$PSScriptRoot\package\$module" -Force
Copy-Item "$PSScriptRoot\$module\$module.psm1" -Destination "$PSScriptRoot\package\$module" -Force

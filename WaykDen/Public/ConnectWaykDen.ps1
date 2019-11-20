function Connect-WaykDen(
[Parameter(Mandatory=$true)]
[string] $DenUrl,
[switch]$Force,
[string] $ApiKey
){
    $result = Invoke-WebRequest -Uri $DenUrl/health -Method GET
    if($result.StatusCode -EQ 200){
        if ($ApiKey) {
            $Env:DEN_API_KEY = $ApiKey
        }
        else{
            if($Force){
                Connect-WaykDenUser -Force $DenUrl
            }else{
                Connect-WaykDenUser $DenUrl
            }
        }

        $Env:DEN_SERVER_URL = $DenUrl
        Write-Host "Success! Server URL: " $DenUrl
    }
    else{
        throw "Having trouble reaching " + $DenUrl
    }
}

Export-ModuleMember -Function Connect-WaykDen
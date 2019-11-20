function Connect-WaykDenUser(
[Parameter(Mandatory=$true)]
[string] $DenUrl,
[switch]$Force,
[string] $ApiKey
){
    $result = Invoke-WebRequest -Uri $DenUrl/health -Method GET
    if($result.StatusCode -EQ 200){
        if ($ApiKey) {
            $Env:DEN_API_KEY = $ApiKey
            $Env:DEN_ACCESS_TOKEN = ""
            $Env:DEN_REFRESH_TOKEN = ""
        }
        else{
            $Env:DEN_API_KEY = ""
            if($Force){
                Connect-WaykDenLucidUser -Force $DenUrl
            }else{
                Connect-WaykDenLucidUser $DenUrl
            }
        }

        $Env:DEN_SERVER_URL = $DenUrl
        Write-Host "Success! Server URL: " $DenUrl
    }
    else{
        throw "Having trouble reaching " + $DenUrl
    }
}

Export-ModuleMember -Function Connect-WaykDenUser
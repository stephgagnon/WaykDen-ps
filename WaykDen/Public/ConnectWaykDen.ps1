function Connect-WaykDen(
[string] $ApiKey,
[string] $ServerUrl,
[switch] $UseUserLogin
){
    $config = $null
    if(!($ApiKey) -OR !($ServerUrl)){
        $config = Get-WaykDenConfig -PwshObject
    }

    if($config){
        if(!($ApiKey)){
            $ApiKey = $config.DenServerConfigObject.ApiKey;
        }
        if(!($ServerUrl)){
            $ServerUrl = $config.DenServerConfigObject.ExternalUrl
        }
    }

    $result = Invoke-WebRequest -Uri $ServerUrl/health -Method GET
    if($result.StatusCode -EQ 200){
        if ($UseUserLogin) {
            Connect-WaykDenUser -Force
        }
        else{
            $Env:DEN_API_KEY = $ApiKey
        }
        $Env:DEN_SERVER_URL = $ServerUrl
        Write-Host "Success! Server URL: " $ServerUrl
    }
    else{
        throw "Having trouble reaching " + $ServerUrl
    }
}

Export-ModuleMember -Function Connect-WaykDen
class WaykNowInfo
{
	[string] $DataPath
	[string] $GlobalPath
	[string] $GlobalDataPath
	[string] $ConfigFile
	[string] $DenPath
	[string] $DenGlobalPath
	[string] $LogPath
	[string] $LogGlobalPath
	[string] $CertificateFile
	[string] $PrivateKeyFile
	[string] $PasswordVault
	[string] $KnownHostsFile
	[string] $BookmarksFile
}

function Get-WaykNowInfo()
{
	$DataPath = '';
	$GlobalDataPath = '';
	$GlobalPath = '';
	$resolvedGlobalPath = '';
	if (Get-IsWindows)	{
		Add-PathIfNotExist "$Env:APPDATA\Wayk" $true
		Add-PathIfNotExist "$Env:APPDATA\Wayk\den" $true

		$DataPath = $Env:APPDATA + '\Wayk';
		if (Get-Service "WaykNowService" -ErrorAction SilentlyContinue)	{
			if(Get-IsRunAsAdministrator)	{
				Add-PathIfNotExist "$Env:ALLUSERSPROFILE\Wayk" $true
				Add-PathIfNotExist "$Env:ALLUSERSPROFILE\Wayk\WaykNow.cfg" $false
				Add-PathIfNotExist "$Env:ALLUSERSPROFILE\Wayk\logs" $true
			}

			$LogGlobalPath = "$Env:ALLUSERSPROFILE\Wayk\logs" 
			$GlobalDataPath = $Env:ALLUSERSPROFILE + '\Wayk\WaykNow.cfg'
			$GlobalDenPath = $Env:ALLUSERSPROFILE + '\Wayk\den'
			$resolvedGlobalPath = Resolve-Path -Path $GlobalDataPath
			$resolvedLogGlobalPath = Resolve-Path -Path $LogGlobalPath
			$resolvedGlobalDenPath = Resolve-Path -Path $GlobalDenPath
			$GlobalPath = Resolve-Path -Path ($Env:ALLUSERSPROFILE + '\Wayk')
		}
	} elseif ($IsMacOS) {
		Add-PathIfNotExist "~/Library/Application Support/Wayk" $true
		$DataPath = '~/Library/Application Support/Wayk'
	} elseif ($IsLinux) {
		Add-PathIfNotExist "~/.config/Wayk" $true
		$DataPath = '~/.config/Wayk'
	}

	$resolvedPath = Resolve-Path -Path $DataPath

	Add-PathIfNotExist "$resolvedPath/WaykNow.cfg" $false
	Add-PathIfNotExist "$resolvedPath/logs" $true
	Add-PathIfNotExist "$resolvedPath/bookmarks" $true
	Add-PathIfNotExist "$resolvedPath/den" $true

	Add-PathIfNotExist "$resolvedPath/WaykNow.crt" $false
	Add-PathIfNotExist "$resolvedPath/WaykNow.key" $false
	Add-PathIfNotExist "$resolvedPath/WaykNow.vault" $false
	Add-PathIfNotExist "$resolvedPath/known_hosts" $false

	$WaykNowInfoObject = [WaykNowInfo]::New()
	$WaykNowInfoObject.DataPath = $resolvedPath
	$WaykNowInfoObject.GlobalPath = $GlobalPath
	$WaykNowInfoObject.GlobalDataPath = $resolvedGlobalPath
	$WaykNowInfoObject.ConfigFile =  Resolve-Path -Path "$resolvedPath/WaykNow.cfg" 
	$WaykNowInfoObject.DenPath = "$resolvedPath/den"
	$WaykNowInfoObject.DenGlobalPath = $resolvedGlobalDenPath
	$WaykNowInfoObject.LogPath =  Resolve-Path -Path "$resolvedPath/logs" 
	$WaykNowInfoObject.LogGlobalPath =  $resolvedLogGlobalPath
	$WaykNowInfoObject.CertificateFile =  Resolve-Path -Path "$resolvedPath/WaykNow.crt" 
	$WaykNowInfoObject.PrivateKeyFile =   Resolve-Path -Path "$resolvedPath/WaykNow.key" 
	$WaykNowInfoObject.PasswordVault =  Resolve-Path -Path "$resolvedPath/WaykNow.vault" 
	$WaykNowInfoObject.KnownHostsFile =  Resolve-Path -Path "$resolvedPath/known_hosts" 
	$WaykNowInfoObject.BookmarksFile = Resolve-Path -Path "$resolvedPath/bookmarks"

	return $WaykNowInfoObject 
}
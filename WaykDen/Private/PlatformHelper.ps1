function Get-IsWindows
{
    if (-Not (Test-Path 'variable:global:IsWindows')) {
        return $true # Windows PowerShell 5.1 or earlier
    } else {
        return $IsWindows
    }
}

# Work only with windows, use the check Get-IsWindows before call this one
function Get-IsRunAsAdministrator  
{
    return ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')
}
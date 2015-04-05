Import-Module .\Octopus.Cmdlets\bin\Debug\Octopus.Cmdlets.dll -Prefix Octo

Get-OctoVariable
Get-OctoVariable -Name Test

Remove-Module Octopus.Cmdlets

Octopus-Cmdlets
===============

PowerShell cmdlets to simplify and automate working with an Octopus Deploy server.


Installation
============
* Build the solution
* Open PowerShell and run
  Import-Module "[Path to the build dll]"
  

Usage
=====
First you have to create a connection to the server. If you haven't already, create an ApiKey 
(see: http://docs.octopusdeploy.com/display/OD/How+to+create+an+API+key). Then connect with
> Connect-OctoServer -Server http://OctopusDeployUrl -ApiKey [key]

Get-OctoEnvironments: List the environments defined on the octopus server
Get-Projects: Lists all the projects defined on the server
Get-Variables -Project [project]: Lists all the variables in a given project
Add-Variable -Project [project] -Name [name] -Value [value] [-Environments @()] [-Sensitive]
Remove-Variable -Project [project] -Name [name]: Removes the first variable with that name



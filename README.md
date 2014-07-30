Octopus-Cmdlets
===============

Yo Dawg, I herd you like automation, so I put cmdlets in your PowerShell so you can automate while you automate.

PowerShell cmdlets to simplify and automate working with an Octopus Deploy server.

<sub>Automate all the things!!!<sub>

Installation
============
* Build the solution
* Open PowerShell and run:

  Import-Module "[Path to the built dll]"

Usage
=====
First you have to create a connection to the server. If you haven't already, create an ApiKey 
(see: http://docs.octopusdeploy.com/display/OD/How+to+create+an+API+key). Then connect with
  Connect-OctoServer -Server \<string\> -ApiKey \<string\>

List the environments defined on the octopus server:
  Get-OctoEnvironments

Lists all the projects defined on the server:
  Get-OctoProjects

Lists all the variables in a given project
  Get-OctoVariables -Project \<string\>

Add a variable to a project's VariableSet:
  Add-OctoVariable -Project \<string\> -Name \<string\> -Value \<string\> \[-Environments \<string\[\]\>\] \[-Sensitive\]

Removes the first variable with a given name:
  Remove-OctoVariable -Project \<string\> -Name \<string\>



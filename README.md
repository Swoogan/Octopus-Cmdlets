Octopus-Cmdlets
===============

Yo Dawg, I herd you like automation, so I put cmdlets in your PowerShell so you can automate while you automate.

PowerShell cmdlets to simplify and automate working with an Octopus Deploy server.

<sub>Automate all the things!!!<sub>

Installation
============
* Build the solution
* Open PowerShell and run:

    Import-Module "[Path to the built dll]" -Prefix Octo

Usage
=====
First you have to create a connection to the server. If you haven't already, create an ApiKey 
(see: http://docs.octopusdeploy.com/display/OD/How+to+create+an+API+key). Then connect with:

    Connect-OctoServer -Server <string> -ApiKey <string>

List the environments defined on the octopus server:

    Get-OctoEnvironment [-Name <string[]>]

Lists all the projects defined on the server:

    Get-OctoProject [-Name <string[]>]

Lists all the variables in a given project

    Get-OctoVariable -Project <string> [-Name <string[]>]

Add a variable to a project's VariableSet:

    Add-OctoVariable -Project <string> -Name <string> -Value <string> [-Environments <string[]>] [-Sensitive]

Removes the first variable with a given name:

    Remove-OctoVariable -Project <string> -Name <string>

Licence
=======
Copyright 2014 Colin Svingen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

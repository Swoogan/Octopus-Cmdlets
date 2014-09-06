Octopus-Cmdlets
===============

Yo Dawg, I herd you like automation, so I put cmdlets in your PowerShell so you can automate while you automate.

PowerShell cmdlets to simplify and automate working with an Octopus Deploy server.

<sub>Automate all the things!!!<sub>

Installation
============
* Option 1:
	* Open the solution in Visual Studio and build it (this will restore the psake nuget package).
	* Open the Package Manager Console and run:

		`psake Install`

* Option 2 (if you have psake):	
	* Open a prompt in the solution folder and type `psake Install`

* Option 3 (if psake has already been restored):	
	* Open a PowerShell prompt in the solution folder and type 
	
	packages\psake.[version]\tools\psake.ps1 Install
	
Usage
=====
Please see the [wiki][https://github.com/Swoogan/Octopus-Cmdlets/wiki]

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

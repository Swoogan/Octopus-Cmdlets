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
Please see the [wiki](https://github.com/Swoogan/Octopus-Cmdlets/wiki).

Licence
=======
Copyright 2014 Colin Svingen

   Licensed under the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0)

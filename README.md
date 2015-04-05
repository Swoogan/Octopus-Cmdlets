Octopus-Cmdlets
===============
Yo Dawg, I herd you like automation, so I put cmdlets in your PowerShell so you
can automate while you automate.

Octopus-Cmdlets is a suite of PowerShell cmdlets that enable you to simplify 
and automate your interactions with an Octopus Deploy server.

<sub>Automate all the things!!!<sub>

Note
====
The module name has changed from `Octopus.Cmdlets` to `Octopus-Cmdlets`. Please 
adjust your process (and scripts) accordingly.

Installation
============
PsGet (preferred method)
------------------------
If you don't already have PsGet install, get it from https://github.com/psget/psget

To install Octopus-Cmdlets run

  install-module Octopus-Cmdlets

Binary Archive
--------------
Download the latest binary: [v0.1.0](https://github.com/Swoogan/Octopus-Cmdlets/releases/download/v0.1.0/Octopus-Cmdlets-v0.1.0.zip)

Extract the zip and copy the `Octopus-Cmdlets` folder into a folder in your
`$env:PSModulePath`.

Alternatively, you can copy the extracted folder to wherever you like and add
the full path to your `$env:PSModulePath`.

psake
-----
* Option 1:
	* Open the solution in Visual Studio and build it (this will restore the psake nuget package).
	* Open the Package Manager Console and run:

		`psake install`

* Option 2 (if you have psake):	
	* Open a prompt in the solution folder and type `psake install`

* Option 3 (if psake has already been restored):	
	* Open a PowerShell prompt in the solution folder and type 
	
    `packages\psake.[version]\tools\psake.ps1 install`
	
Usage
=====

    Import-Module Octopus-Cmdlets

Please see the [wiki](https://github.com/Swoogan/Octopus-Cmdlets/wiki) for a 
description of the individual cmdlets, or type `help [cmdlet]` from PowerShell.

Licence
=======
Copyright 2014 Colin Svingen

   Licensed under the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0)

Octopus-Cmdlets
===============
[![Build status](https://ci.appveyor.com/api/projects/status/i5xjnh3ar642j05p?svg=true)](https://ci.appveyor.com/project/Swoogan/octopus-cmdlets)

Yo Dawg, I herd you like automation, so I put cmdlets in your PowerShell so you
can automate while you automate.

Octopus-Cmdlets is a suite of PowerShell cmdlets that enable you to simplify 
and automate your interactions with an Octopus Deploy server.

Note
====
The module name has changed from `Octopus.Cmdlets` to `Octopus-Cmdlets`. Please 
adjust your process (and scripts) accordingly.

Installation
============
Powershell Gallery (preferred method)
-------------------------------------

    Install-Module -Name Octopus-Cmdlets


Binary Archive
--------------
Download the latest binary: [v0.5.1](https://github.com/Swoogan/Octopus-Cmdlets/releases/download/v0.5.1/Octopus-Cmdlets-v0.5.1.zip)

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
Copyright 2018 Colin Svingen

   Licensed under the [Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0)

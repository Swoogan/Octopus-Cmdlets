Todo v0.1.0
-----------
- Write help file
- Improve online documentation
- Chocolaty package
- Add module to PSGet

Todo vNext
----------
- *Add-Step*, Add-Machine, Add-Release
- Remove-Step, Remove-Machine, Remove-Release
- Update-Project, Update-ProjectGroup, Update-Step, Update-Machine, Update-Environment, Update-Release
- Fix removing duplicates from Remove-Variable
- Add Environments and Retention policy to Add-ProjectGroup
- Need some tests. People are starting to use this thing.
- Get-DeploymentProcess by project name or id
- Add cmdlet to Add Step from Template
- Implement duplicate variable finding algorithm
	- Warn if adding a duplicate variable
	- Add switch(es) to Add-Variable to -nowarn -skip and -error on duplicates
- Add verbose output
- Add username/password fields for Connect-Server
- Add way to connect to more than one server at once?
- How do we get all deployment processes (just Get-OctoProject | Get-OctoDeploymentProcess ?)


Caching
-------
- Improve the caching to be per value?
- Actually, give some more thought to this whole caching thing, would per value be an improvement?
- Per value doesn't work when you want a list. How do you know if the whole list is stale?
- Caching by default is probably the problem. Only really need it for ScriptProperties. Change NoCache? to Cache?
- Very annoying to add a new environment, Get-OctoEnvironment and not see it.

Hold
----
- Add Ability to get Step By Id without knowing the project (they're guids they should be globally unique)
	- Doesn't look like the api supports this, would have to iterate over projects, then all actions in the process

Done
----
- Extend Add-Variable to add library vs variables
- Add cmdlet to Clone projects (Copy-Project)
- Add project caching to Get-Project
- Add project caching
- Remove-Environment
- Remove-ProjectGroup
- Move documentation to wiki
- Get-DeploymentProcess
- Remove-Project
- Add-Environment
- Have a shared cache?
- Add-ProjectGroup
- Added ability to add projects with a project group name instead of Id
- Add extension method to Release object to get by version
- Add machines and steps to Add-Variable
- Add roles to Add-Variable
- Mention psake method for build and install in documentation
- Add ScopeName script property to Get-Variable
- Implement Get-Action
- Implement Get-MachineRole 
- Add getting of library variables

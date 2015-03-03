Todo v0.1.0
-----------
- Write help file
- Improve online documentation
- Chocolaty package
- Add module to PSGet

Todo vNext
----------
- Get-Step, Get-Feed
- Add-Step, Add-Machine, Add-Release, Add-Feed
- Remove-Step, Remove-Machine, Remove-Release, Remove-Feed
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
	- Not likely, getting all is one round trip to the server, getting per-value means N round trips
	- So if N is small, getting all might be slower. If N is large or has lots of duplicates, getting all is faster
- Per value doesn't work when you want a list. How do you know if the whole list is stale?

Hold
----
- Add Ability to get Step By Id without knowing the project (they're guids they should be globally unique)
	- Doesn't look like the api supports this, would have to iterate over projects, then all actions in the process

Done
----
- Get-Deployment
- Caching by default is probably the problem. Only really need it for ScriptProperties. Change NoCache? to Cache?
- Add cmdlet to clone deployment step (Copy-Step)
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

Todo
----
- *Add-Step*, Add-Machine, Add-Release
- Remove-Step, Remove-Machine, Remove-Release
- Extend Add-Variable to add library vs variables
- Write help file
- Improve online documentation
- Update-Project, Update-ProjectGroup, Update-Step, Update-Machine, Update-Environment, Update-Release
- Fix removing duplicates from Remove-Variable
- Add Environments and Retention policy to Add-ProjectGroup
- Improve the caching to be per value (Give some more thought to this whole caching thing)
- Need some tests. People are starting to use this thing.
- Get-DeploymentProcess by project name or id
- Add cmdlet to Clone projects
- Add cmdlet to Add Step from Template
- Warn if adding a duplicate variable
- Add switch(es) to Add-Variable to -nowarn -skip and -error on duplicates
- Add verbose output
- Add username/password fields for Connect-Server
- Add way to connect to more than one server at once?

Hold
----
- Add Ability to get Step By Id without knowing the project (they're guids they should be globally unique)
	- Doesn't look like the api supports this, would have to iterate over projects, then all actions in the process

Done
----
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

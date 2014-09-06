Todo
----
- Extend Add-Variable to add library vs variables
- Add-Environment, *Add-Step*, Add-Machine, Add-Release
- Add cmdlet to Clone projects
- Warn if adding a duplicate variable
- Add switch(es) to Add-Variable to -nowarn -skip and -error on duplicates
- Write help file
- Improve online documentation and move to wiki
- Have a shared cache?

Hold
----
- Add Ability to get Step By Id without knowing the project (they're guids they should be globally unique)
	- Doesn't look like the api supports this, would have to iterate over projects, then all actions in the process

Done
----
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

Todo
----
- Extend Add-Variable to add library vs variables
- Add-Environment, *Add-Step*, Add-Machine, Add-Release
- Write help file
- Improve online documentation and move to wiki
- Improve the caching to be per value
- Need some test. People are starting to use this thing.
- Add cmdlet to Clone projects
- Add cmdlet to Add Step from Template
- Warn if adding a duplicate variable
- Add switch(es) to Add-Variable to -nowarn -skip and -error on duplicates

Hold
----
- Add Ability to get Step By Id without knowing the project (they're guids they should be globally unique)
	- Doesn't look like the api supports this, would have to iterate over projects, then all actions in the process

Done
----
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

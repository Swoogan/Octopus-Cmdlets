Iteration 2
===========
- Add-Step
- Add-Release

Iteration 3 (v0.2.0)
====================
- More detailed tests (UpdateVariable, AddVariable, AddLibraryVariable)
- More examples

Iteration 4
===========
- Pipeline tests
- Complex Examples

Iteration 5 (v0.3.0)
====================
- Remove-Step, Remove-Machine, Remove-MachineRole, Remove-Release, Remove-Feed

Backlog
=======
- Update-Project, Update-ProjectGroup, Update-Step, Update-Machine, Update-Environment, Update-Release, Update-Feed
- Lifecycles
- Copy Step to a different project
- Find-Unreplaced and other utility cmdlets
- Fix removing duplicates from Remove-Variable
- Get-DeploymentProcess by project name or id
- Add cmdlet to Add Step from Template
- Implement duplicate variable finding algorithm
	- Warn if adding a duplicate variable
	- Add switch(es) to Add-Variable to -nowarn -skip and -error on duplicates
- Add verbose output
- Add username/password fields for Connect-Server
- Add way to connect to more than one server at once?
- How do we get all deployment processes (just Get-OctoProject | Get-OctoDeploymentProcess ?)
- Chocolaty package
- InnoSetup installer
- Add aliases

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
====
- More detailed tests (CopyProject)
- Get-Step
- Add-Machine
- More detailed tests: CopyStep
- Add-Feed
- Examples
- Basic tests
- Appveyor
- Create zip file
- Add module to PSGet

properties {
  $proj = "Octopus-Cmdlets/Octopus-Cmdlets.csproj"
  $installPath = "Octopus-Cmdlets"
}

function Copy-Files([string]$path) {
	Copy-Item -Path ./Octopus-Cmdlets/Octopus-Cmdlets.Format.ps1xml -Destination $path
	Copy-Item -Path ./Octopus-Cmdlets/Octopus-Cmdlets.psd1 -Destination $path
	Copy-Item -Path ./Octopus-Cmdlets/Octopus-Cmdlets.Types.ps1xml -Destination $path

	if (-not (Test-Path $path/en-US)) { mkdir $path/en-US }
	Move-Item -Path $path/Octopus-Cmdlets.dll-help.xml -Destination $path/en-US/Octopus-Cmdlets.dll-help.xml -Force
}

function Install([string]$destination) {
	if (-not (Test-Path $destination)) {
		New-Item -Path $destination -ItemType directory | Out-Null
		Write-Host "Created directory '$destination'" -ForegroundColor Green
	}
	Copy-Item -Path ./Octopus-Cmdlets/bin/Release/* -Destination $destination
	Copy-Files $destination
}

task default -depends Release

task Release { 
  msbuild $proj /p:Configuration=Release /p:SolutionDir=$PSScriptRoot
}

task Install -depends Install-Personal, Release 

task Install-Personal -depends Release {
	$paths = $env:PSModulePath.Split(';')
	$personal = $paths[0]	# suppose it's not guaranteed to be the first, oh well...
	$destination = Join-Path $personal -ChildPath $installPath
	Install $destination
}

task Install-System -depends Release {
	$paths = $env:PSModulePath.Split(';')
	$system = $paths[1]	# suppose it's not guaranteed to be the second, oh well...
	$destination = Join-Path $system -ChildPath $installPath
	Install $destination
}

task Clean { 
  msbuild $proj  /p:Configuration=Release /target:Clean
}

task Debug { 
  msbuild $proj
}

task DebugClean { 
  msbuild $proj /target:Clean
}

task ? -Description "Helper to display task info" {
	Write-Documentation
}

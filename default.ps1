properties {
  
}

task default -depends Release

task Release { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj /p:Configuration=Release
}

task Install -depends Install-ForMe, Release 

task Install-ForMe -depends Release {
	$paths = $env:PSModulePath.Split(';')
	$personal = $paths[0]	# suppose it's not guaranteed to be the first, oh well...
	$installPath = Join-Path $personal -ChildPath "Octopus.Cmdlets"
	if (-not (Test-Path $installPath)) {
		New-Item -Path $installPath -ItemType directory | Out-Null
		Write-Host "Created directory '$installPath'" -ForegroundColor Green
	}
	Copy-Item -Path ./Octopus.Cmdlets/bin/Release/* -Destination $installPath
}

task Install-ForEveryone {
	$paths = $env:PSModulePath.Split(';')
	$personal = $paths[1]	# suppose it's not guaranteed to be the second, oh well...
	$installPath = Join-Path $personal -ChildPath "Octopus.Cmdlets"
	if (-not (Test-Path $installPath)) {
		New-Item -Path $installPath -ItemType directory | Out-Null
		Write-Host "Created directory '$installPath'" -ForegroundColor Green
	}
	Copy-Item -Path ./Octopus.Cmdlets/bin/Release/* -Destination $installPath
}

task Clean { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj  /p:Configuration=Release /target:Clean
}

task Debug { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj
}

task DebugClean { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj /target:Clean
}

task ? -Description "Helper to display task info" {
	Write-Documentation
}
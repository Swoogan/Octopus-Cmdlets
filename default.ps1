properties {
  $proj = "Octopus.Cmdlets/Octopus.Cmdlets.csproj"
  $installPath = "Octopus.Cmdlets"
}

task default -depends Release

task Release { 
  msbuild $proj /p:Configuration=Release
}

task Install -depends Install-ForMe, Release 

task Install-ForMe -depends Release {
	$paths = $env:PSModulePath.Split(';')
	$personal = $paths[0]	# suppose it's not guaranteed to be the first, oh well...
	$installPath = Join-Path $personal -ChildPath $installPath
	if (-not (Test-Path $installPath)) {
		New-Item -Path $installPath -ItemType directory | Out-Null
		Write-Host "Created directory '$installPath'" -ForegroundColor Green
	}
	Copy-Item -Path ./Octopus.Cmdlets/bin/Release/* -Destination $installPath
}

task Install-ForEveryone {
	$paths = $env:PSModulePath.Split(';')
	$personal = $paths[1]	# suppose it's not guaranteed to be the second, oh well...
	$installPath = Join-Path $personal -ChildPath $installPath
	if (-not (Test-Path $installPath)) {
		New-Item -Path $installPath -ItemType directory | Out-Null
		Write-Host "Created directory '$installPath'" -ForegroundColor Green
	}
	Copy-Item -Path ./Octopus.Cmdlets/bin/Release/* -Destination $installPath
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
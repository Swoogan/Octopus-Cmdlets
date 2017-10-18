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
    
    Copy-Item -Path ./Octopus-Cmdlets/bin/Release/net45/* -Destination $destination
    Copy-Files $destination
}

task default -depends Release

task Release { 
  exec { Push-Location $PSScriptRoot; dotnet build $proj --configuration Release; Pop-Location }
}

task Install -depends Install-Personal, Release 

task Install-Personal -depends Release {
    $paths = $env:PSModulePath.Split(';')
    $personal = $paths[0]	# suppose it's not guaranteed to be the first, oh well...
    $destination = Join-Path $personal $installPath
    Install $destination
}

task Install-System -depends Release {
    $paths = $env:PSModulePath.Split(';')
    $system = $paths[1]	# suppose it's not guaranteed to be the second, oh well...
    $destination = Join-Path $system $installPath
    Install $destination
}

task Clean { 
  exec { dotnet build $proj --configuration Release --no-incremental }
}

task Debug { 
  exec { dotnet build $proj }
}

task DebugClean { 
  exec { dotnet build $proj --no-incremental }
}

task ? -Description "Helper to display task info" {
    Write-Documentation
}

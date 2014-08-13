properties {
  
}

task default -depends Release

task Release { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj /p:Configuration=Release
}

task Install -depends Release {
	
}

task Debug { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj /p:Configuration=Debug
}

task Clean { 
  msbuild Octopus.Cmdlets/Octopus.Cmdlets.csproj /target:Clean
}

task ? -Description "Helper to display task info" {
	Write-Documentation
}
$ReadmePath = Join-Path $PSScriptRoot 'README.md'
If (-not (Test-Path $ReadmePath)) {
    Write-Error 'Unable to find README.md'
    Exit 1
}

$ReadmeContents = Get-Content $ReadmePath -Raw
[regex]$Regex = 'dotnet tool install --global https --version (.+)-\*'
$Match = $Regex.Match($ReadmeContents)
If (-not $Match.Success) {
    Write-Error 'Unable to find version information in README.md'
    Exit 1
}

$HttpsCsprojPath = Join-Path $PSScriptRoot 'src/https/https.csproj'
If (-not (Test-Path $HttpsCsprojPath)) {
    Write-Error 'Unable to find https.csproj'
    Exit 1
}

[xml]$HttpsCsproj = Get-Content $HttpsCsprojPath

$Expected = $HttpsCsproj.Project.PropertyGroup.VersionPrefix |
    Where-Object { $null -ne $_ } |
    ForEach-Object { $_.ToString().Trim() }
$Actual = $Match.Groups[1].ToString().Trim()
If ($Expected -ne $Actual) {
    Write-Error "Expected to have ${Expected} version in README.md, but found ${Actual}"
    Exit 1
}

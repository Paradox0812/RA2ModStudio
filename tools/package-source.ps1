param(
    [string]$OutputDirectory = "artifacts",
    [string]$PackageName = "RA2IniEditor-source.zip",
    [switch]$CleanFirst
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$outputRoot = Join-Path $repositoryRoot $OutputDirectory
$packagePath = Join-Path $outputRoot $PackageName
$stagingRoot = Join-Path $env:TEMP ("RA2IniEditor_SourcePackage_" + [guid]::NewGuid().ToString("N"))

$excludedDirectories = @(
    ".git",
    ".vs",
    "bin",
    "obj",
    "TestResults",
    "artifacts",
    "Logs",
    "publish"
)

$excludedFilePatterns = @(
    "*.user",
    "*.suo",
    "*.vsidx",
    "*.DotSettings.user",
    "*.log",
    "*.tmp",
    "*.cache",
    "*.bak",
    "*.orig",
    "*_wpftmp*",
    "*.nupkg",
    "*.snupkg"
)

function Test-IsExcludedDirectory {
    param([System.IO.DirectoryInfo]$Directory)

    return $excludedDirectories -contains $Directory.Name
}

function Test-IsExcludedFile {
    param([System.IO.FileInfo]$File)

    foreach ($pattern in $excludedFilePatterns) {
        if ($File.Name -like $pattern) {
            return $true
        }
    }

    return $false
}

function Remove-ExcludedContent {
    param([string]$Path)

    foreach ($directoryName in $excludedDirectories) {
        Get-ChildItem -Path $Path -Directory -Recurse -Force -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -eq $directoryName } |
            Sort-Object FullName -Descending |
            Remove-Item -Recurse -Force
    }

    Get-ChildItem -Path $Path -File -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { Test-IsExcludedFile -File $_ } |
        Sort-Object FullName -Descending |
        Remove-Item -Force
}

try {
    if ($CleanFirst) {
        & (Join-Path $PSScriptRoot "clean.ps1")
    }

    New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

    Get-ChildItem -Path $repositoryRoot -Force | Where-Object {
        if ($_.PSIsContainer) {
            -not (Test-IsExcludedDirectory -Directory $_)
        }
        else {
            -not (Test-IsExcludedFile -File $_)
        }
    } | ForEach-Object {
        $destination = Join-Path $stagingRoot $_.Name
        if ($_.PSIsContainer) {
            Copy-Item -Path $_.FullName -Destination $destination -Recurse -Force -Container -Exclude $excludedFilePatterns
            Remove-ExcludedContent -Path $destination
        }
        else {
            Copy-Item -Path $_.FullName -Destination $destination -Force
        }
    }

    if (Test-Path $packagePath) {
        Remove-Item -Path $packagePath -Force
    }

    Compress-Archive -Path (Join-Path $stagingRoot "*") -DestinationPath $packagePath -Force
    Write-Host "Source package created: $packagePath"
}
finally {
    if (Test-Path $stagingRoot) {
        Remove-Item -Path $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

param(
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $repositoryRoot

$excludedDirectoryNames = @(
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

function Test-IsInsideRepository {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($repositoryRoot)
    return $fullPath.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)
}

function Remove-RepositoryItem {
    param([System.IO.FileSystemInfo]$Item)

    if (-not (Test-IsInsideRepository -Path $Item.FullName)) {
        throw "Refusing to remove path outside repository: $($Item.FullName)"
    }

    if ($WhatIf) {
        Write-Host "Would remove $($Item.FullName)"
        return
    }

    Remove-Item -LiteralPath $Item.FullName -Recurse:$Item.PSIsContainer -Force
}

Get-ChildItem -Path $repositoryRoot -Recurse -Force -Directory |
    Where-Object { ($excludedDirectoryNames -contains $_.Name) -or $_.Name -like ".verify-*" } |
    Sort-Object FullName -Descending |
    ForEach-Object { Remove-RepositoryItem -Item $_ }

Get-ChildItem -Path $repositoryRoot -Recurse -Force -File |
    Where-Object {
        $file = $_
        foreach ($pattern in $excludedFilePatterns) {
            if ($file.Name -like $pattern) {
                return $true
            }
        }

        return $false
    } |
    Sort-Object FullName -Descending |
    ForEach-Object { Remove-RepositoryItem -Item $_ }

if ($WhatIf) {
    Write-Host "RA2IniEditor source tree clean preview completed."
}
else {
    Write-Host "RA2IniEditor source tree cleaned."
}

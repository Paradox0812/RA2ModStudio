param(
    [ValidateSet("Full", "IdeOnly")]
    [string]$Profile = "IdeOnly",
    [string]$OutputDirectory = "artifacts",
    [string]$PackageName = "RA2IniEditor.SourceClean.zip"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if (-not $PSBoundParameters.ContainsKey("PackageName") -and $Profile -eq "IdeOnly") {
    $PackageName = "RA2IniEditor.IDE.SourceClean.zip"
}

$outputRoot = Join-Path $repositoryRoot $OutputDirectory
$packagePath = Join-Path $outputRoot $PackageName
$stagingRoot = Join-Path $env:TEMP ("RA2IniEditor_SourceClean_" + [guid]::NewGuid().ToString("N"))

$excludedDirectoryNames = @(
    ".git",
    ".vs",
    "bin",
    "obj",
    "artifacts",
    "TestResults",
    ".coverage",
    "coverage",
    "Logs",
    "publish"
)

$excludedFilePatterns = @(
    "*.user",
    "*.suo",
    "*.rsuser",
    "*.userosscache",
    "*.sln.docstates",
    "*.vsidx",
    "*.DotSettings.user",
    "*.nupkg",
    "*.snupkg",
    "*.zip",
    "*.7z",
    "*.rar",
    "*.log",
    "*.tmp",
    "*.cache",
    "*.bak",
    "*.orig",
    "*_wpftmp*",
    "secrets.json",
    "*.secrets.json",
    "appsettings.Local.json",
    "appsettings.*.Local.json"
)

$allowedEnvironmentTemplateNames = @(
    ".env.example",
    ".env.sample",
    ".env.template"
)

$fullRequiredEntries = @(
    "RA2IniEditor.sln",
    "RA2IniEditor.Core/",
    "RA2IniEditor.Infrastructure/",
    "RA2IniEditor.IDE/",
    "RA2IniEditor.Tests/",
    "RA2IniEditor.UiAutomationTests/",
    "docs/",
    "tools/",
    "RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json"
)

$ideOnlyRequiredEntries = @(
    "RA2IniEditor.IDE.sln",
    "RA2IniEditor.Core/",
    "RA2IniEditor.Infrastructure/",
    "RA2IniEditor.IDE/",
    "RA2IniEditor.Tests/",
    "RA2IniEditor.UiAutomationTests/",
    "docs/",
    "tools/",
    "IDE_ONLY_PACKAGE_NOTE.md",
    "RA2IniEditor.Infrastructure/FieldRegistry/BuiltIn/builtin-yr-ares-phobos-fallback-v3.2.fields.json"
)

$forbiddenEntryPatterns = @(
    "(^|/)\.git/",
    "(^|/)\.vs/",
    "(^|/)bin/",
    "(^|/)obj/",
    "(^|/)artifacts/",
    "(^|/)TestResults/",
    "(^|/)\.coverage/",
    "(^|/)coverage/",
    "(^|/)Logs/",
    "(^|/)publish/",
    "(^|/)\.verify-[^/]+/",
    "(^|/)\.env($|\.(?!example$|sample$|template$)[^/]+$)",
    "(^|/)(secrets\.json|[^/]+\.secrets\.json|appsettings\.Local\.json|appsettings\.[^/]+\.Local\.json)$",
    "\.(user|suo|rsuser|userosscache|vsidx|log|tmp|cache|bak|orig|nupkg|snupkg|zip|7z|rar)$",
    "\.sln\.docstates$",
    "\.DotSettings\.user$",
    "_wpftmp"
)

$ideOnlyForbiddenEntryPatterns = @(
    "^RA2IniEditor\.sln$",
    "^RA2IniEditor\.csproj$",
    "^App\.xaml(\.cs)?$",
    "^MainWindow\.xaml(\.cs)?$",
    "^ProjectToolsWindow\.xaml(\.cs)?$",
    "^(Analysis|RA2|Services|ViewModels|Views|Project|Schema|Dictionaries|Themes|Assets|Converters|Models|UI)/"
)

$ideOnlyExcludedRootEntries = @(
    "RA2IniEditor.sln",
    "RA2IniEditor.csproj",
    "App.xaml",
    "App.xaml.cs",
    "MainWindow.xaml",
    "MainWindow.xaml.cs",
    "ProjectToolsWindow.xaml",
    "ProjectToolsWindow.xaml.cs",
    "Analysis",
    "RA2",
    "Services",
    "ViewModels",
    "Views",
    "Project",
    "Schema",
    "Dictionaries",
    "Themes",
    "Assets",
    "Converters",
    "Models",
    "UI"
)

function Test-IsExcludedDirectory {
    param([System.IO.DirectoryInfo]$Directory)

    return ($excludedDirectoryNames -contains $Directory.Name) -or $Directory.Name -like ".verify-*"
}

function Test-IsIdeOnlyExcludedRootEntry {
    param([System.IO.FileSystemInfo]$Entry)

    return $Profile -eq "IdeOnly" -and ($ideOnlyExcludedRootEntries -contains $Entry.Name)
}

function Test-IsExcludedFile {
    param([System.IO.FileInfo]$File)

    if ($allowedEnvironmentTemplateNames -contains $File.Name) {
        return $false
    }

    if ($File.Name -eq ".env" -or $File.Name -like ".env.*") {
        return $true
    }

    foreach ($pattern in $excludedFilePatterns) {
        if ($File.Name -like $pattern) {
            return $true
        }
    }

    return $false
}

function Test-IsLocalRa2IdePath {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $rootPath = [System.IO.Path]::GetFullPath($repositoryRoot).TrimEnd("\", "/") + [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($rootPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $false
    }

    $relativePath = $fullPath.Substring($rootPath.Length)
    $normalizedPath = $relativePath.Replace("\", "/")
    return $normalizedPath.StartsWith(".ra2ide/local/", [System.StringComparison]::OrdinalIgnoreCase)
}

function Remove-ExcludedContent {
    param([string]$Path)

    Get-ChildItem -Path $Path -Directory -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { (Test-IsExcludedDirectory -Directory $_) -or (Test-IsLocalRa2IdePath -Path $_.FullName) } |
        Sort-Object FullName -Descending |
        Remove-Item -Recurse -Force

    Get-ChildItem -Path $Path -File -Recurse -Force -ErrorAction SilentlyContinue |
        Where-Object { Test-IsExcludedFile -File $_ } |
        Sort-Object FullName -Descending |
        Remove-Item -Force
}

function Assert-RequiredEntry {
    param(
        [string[]]$Entries,
        [string]$RequiredEntry
    )

    if ($RequiredEntry.EndsWith("/", [System.StringComparison]::Ordinal)) {
        $hasDirectory = $Entries | Where-Object { $_.StartsWith($RequiredEntry, [System.StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
        if (-not $hasDirectory) {
            throw "Source package is missing required directory: $RequiredEntry"
        }

        return
    }

    if (-not ($Entries -contains $RequiredEntry)) {
        throw "Source package is missing required file: $RequiredEntry"
    }
}

function Assert-NoForbiddenEntry {
    param(
        [string[]]$Entries,
        [string[]]$ForbiddenPatterns
    )

    foreach ($entry in $Entries) {
        foreach ($pattern in $ForbiddenPatterns) {
            if ($entry -match $pattern) {
                throw "Source package contains forbidden entry: $entry"
            }
        }
    }
}

try {
    New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null

    Get-ChildItem -Path $repositoryRoot -Force | Where-Object {
        if (Test-IsIdeOnlyExcludedRootEntry -Entry $_) {
            return $false
        }

        if ($_.PSIsContainer) {
            -not (Test-IsExcludedDirectory -Directory $_)
        }
        else {
            -not (Test-IsExcludedFile -File $_)
        }
    } | ForEach-Object {
        $destination = Join-Path $stagingRoot $_.Name
        if ($_.PSIsContainer) {
            Copy-Item -LiteralPath $_.FullName -Destination $destination -Recurse -Force -Container
            Remove-ExcludedContent -Path $destination
        }
        else {
            Copy-Item -LiteralPath $_.FullName -Destination $destination -Force
        }
    }

    if (Test-Path $packagePath) {
        Remove-Item -LiteralPath $packagePath -Force
    }

    Compress-Archive -Path (Join-Path $stagingRoot "*") -DestinationPath $packagePath -Force

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        $entries = @($zip.Entries | ForEach-Object { $_.FullName.Replace("\", "/") })
        $requiredEntries = if ($Profile -eq "IdeOnly") { $ideOnlyRequiredEntries } else { $fullRequiredEntries }
        foreach ($requiredEntry in $requiredEntries) {
            Assert-RequiredEntry -Entries $entries -RequiredEntry $requiredEntry
        }

        $effectiveForbiddenEntryPatterns = @($forbiddenEntryPatterns)
        if ($Profile -eq "IdeOnly") {
            $effectiveForbiddenEntryPatterns += $ideOnlyForbiddenEntryPatterns
        }

        Assert-NoForbiddenEntry -Entries $entries -ForbiddenPatterns $effectiveForbiddenEntryPatterns
    }
    finally {
        $zip.Dispose()
    }

    Write-Host "Clean source package created: $packagePath"
    Write-Host "Package profile: $Profile"
    Write-Host "Packaged file count: $($entries.Count)"
    Write-Host "Excluded directories: $($excludedDirectoryNames -join ', ')"
    Write-Host "Excluded directory patterns: .verify-*"
    Write-Host "Excluded file patterns: $($excludedFilePatterns -join ', ')"
    Write-Host "Excluded local environment files: .env, .env.* (except templates)"
}
finally {
    if (Test-Path $stagingRoot) {
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

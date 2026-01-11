<#
.SYNOPSIS
    Builds and publishes the Simple.AutoMapper package to NuGet.org.

.DESCRIPTION
    This script automates the NuGet package publishing workflow:
    1. Builds the project in Release mode
    2. Runs all tests (optional)
    3. Creates the NuGet package
    4. Publishes to NuGet.org

.PARAMETER ApiKey
    Your NuGet.org API key. If not provided, the script will look for
    the NUGET_API_KEY environment variable.

    Get your API key from: https://www.nuget.org/account/apikeys

.PARAMETER SkipBuild
    Skip the build step and use existing build artifacts.

.PARAMETER SkipTests
    Skip running tests before publishing.

.PARAMETER DryRun
    Build and package without actually publishing to NuGet.org.
    Useful for testing the package creation process.

.EXAMPLE
    # Quick publish (API key from environment variable)
    .\publish-nuget.ps1

.EXAMPLE
    # Publish with API key
    .\publish-nuget.ps1 -ApiKey "your-api-key-here"

.EXAMPLE
    # Test package creation without publishing
    .\publish-nuget.ps1 -DryRun

.EXAMPLE
    # Quick publish, skip tests
    .\publish-nuget.ps1 -SkipTests

.EXAMPLE
    # Set API key as environment variable (recommended)
    $env:NUGET_API_KEY = "your-api-key-here"
    .\publish-nuget.ps1

.NOTES
    Author: ODINSOFT
    Repository: https://github.com/odinsoft-lab/simple.mapper

    Before first use:
    1. Get your API key from https://www.nuget.org/account/apikeys
    2. Set environment variable: $env:NUGET_API_KEY = "your-key"
    3. Run: .\publish-nuget.ps1

.LINK
    https://www.nuget.org/packages/Simple.AutoMapper/
#>

param(
    [Parameter(Mandatory=$false, HelpMessage="NuGet.org API key")]
    [string]$ApiKey = "",

    [Parameter(Mandatory=$false, HelpMessage="Skip build step")]
    [switch]$SkipBuild = $false,

    [Parameter(Mandatory=$false, HelpMessage="Skip running tests")]
    [switch]$SkipTests = $false,

    [Parameter(Mandatory=$false, HelpMessage="Test run without publishing")]
    [switch]$DryRun = $false
)

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RootDir "src\Simple.Mapper.csproj"
$Configuration = "Release"
$OutputDirectory = Join-Path $RootDir "src\bin\Release"
$NuGetSource = "https://api.nuget.org/v3/index.json"

# Color output functions
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warn { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Err { param($Message) Write-Host $Message -ForegroundColor Red }

# Banner
Write-Host ""
Write-Info "========================================="
Write-Info "  Simple.AutoMapper NuGet Publisher"
Write-Info "========================================="
Write-Host ""
Write-Host "  Usage: .\publish-nuget.ps1 [-ApiKey KEY] [-DryRun] [-SkipTests] [-SkipBuild]"
Write-Host "  Help:  Get-Help .\publish-nuget.ps1 -Detailed"
Write-Host ""

# Check if API key is provided or exists in environment (skip for DryRun)
if (-not $DryRun) {
    if ([string]::IsNullOrEmpty($ApiKey)) {
        $ApiKey = $env:NUGET_API_KEY
        if ([string]::IsNullOrEmpty($ApiKey)) {
            Write-Err "Error: NuGet API key not provided!"
            Write-Host ""
            Write-Host "Please provide API key using one of these methods:"
            Write-Host ""
            Write-Host "  Option 1 - Environment variable (recommended):"
            Write-Host "    `$env:NUGET_API_KEY = 'your-api-key'"
            Write-Host "    .\publish-nuget.ps1"
            Write-Host ""
            Write-Host "  Option 2 - Command parameter:"
            Write-Host "    .\publish-nuget.ps1 -ApiKey 'your-api-key'"
            Write-Host ""
            Write-Host "  Get your API key from: https://www.nuget.org/account/apikeys"
            exit 1
        } else {
            Write-Info "Using API key from environment variable NUGET_API_KEY"
        }
    } else {
        Write-Info "Using API key from command parameter"
    }
} else {
    Write-Warn "DRY RUN MODE - Skipping API key validation"
}

# Check if dotnet CLI is available
try {
    $dotnetVersion = dotnet --version
    Write-Info "Found .NET SDK version: $dotnetVersion"
} catch {
    Write-Err "Error: .NET SDK not found! Please install from https://dotnet.microsoft.com/download"
    exit 1
}

# Clean previous packages
Write-Host ""
Write-Info "Step 1/4: Cleaning previous packages..."
if (Test-Path $OutputDirectory) {
    Remove-Item "$OutputDirectory\*.nupkg" -Force -ErrorAction SilentlyContinue
    Remove-Item "$OutputDirectory\*.snupkg" -Force -ErrorAction SilentlyContinue
}
Write-Success "  Done."

# Build the project
Write-Host ""
if (-not $SkipBuild) {
    Write-Info "Step 2/4: Building project in $Configuration mode..."

    $buildResult = dotnet build "$ProjectPath" --configuration $Configuration 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Err "Build failed! Please fix build errors and try again."
        Write-Host $buildResult
        exit 1
    }
    Write-Success "  Build completed successfully!"
} else {
    Write-Warn "Step 2/4: Skipping build (using existing build)"
}

# Run tests
Write-Host ""
if (-not $SkipTests) {
    Write-Info "Step 3/4: Running tests..."
    $TestPath = Join-Path $RootDir "tests\Mapper.Tests.csproj"

    $testResult = dotnet test "$TestPath" --configuration $Configuration --verbosity quiet 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Warn "  Tests failed! Consider fixing tests before publishing."
        $confirmation = Read-Host "  Do you want to continue anyway? (y/N)"
        if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
            Write-Warn "Publication cancelled"
            exit 1
        }
    } else {
        Write-Success "  All tests passed!"
    }
} else {
    Write-Warn "Step 3/4: Skipping tests"
}

# Create NuGet package
Write-Host ""
Write-Info "Step 4/4: Creating NuGet package..."

$packResult = dotnet pack "$ProjectPath" --configuration $Configuration --no-build 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Err "Package creation failed!"
    Write-Host $packResult
    exit 1
}

# Find the created package
$packageFile = Get-ChildItem -Path $OutputDirectory -Filter "*.nupkg" |
                Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
                Select-Object -First 1

if ($null -eq $packageFile) {
    Write-Err "No package file found in $OutputDirectory"
    exit 1
}

$packagePath = $packageFile.FullName
$packageName = $packageFile.Name

Write-Success "  Package created: $packageName"

# Package summary
Write-Host ""
Write-Info "========================================="
Write-Info "  Package Summary"
Write-Info "========================================="
Write-Host "  Name: $packageName"
Write-Host "  Size: $([math]::Round($packageFile.Length / 1KB, 2)) KB"
Write-Host "  Path: $packagePath"
Write-Host ""

# Publish to NuGet
if ($DryRun) {
    Write-Warn "DRY RUN COMPLETE - Package was NOT published"
    Write-Host ""
    Write-Host "To publish for real, run:"
    Write-Host "  .\publish-nuget.ps1"
    Write-Host ""
} else {
    Write-Warn "Ready to publish to NuGet.org"
    Write-Host ""
    $confirmation = Read-Host "Publish now? (y/N)"
    if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
        Write-Warn "Publication cancelled by user"
        exit 0
    }

    Write-Host ""
    Write-Info "Publishing to NuGet.org..."

    # Capture both stdout and stderr
    $pushOutput = & dotnet nuget push "$packagePath" --api-key "$ApiKey" --source "$NuGetSource" --skip-duplicate 2>&1
    $exitCode = $LASTEXITCODE

    # Check for success
    $outputString = $pushOutput | Out-String
    $isSuccess = $false

    if ($exitCode -eq 0) {
        $isSuccess = $true
    } elseif ($outputString -match "Your package was pushed" -or
              $outputString -match "already exists" -or
              $outputString -match "conflict.*409") {
        $isSuccess = $true
        Write-Warn "Package version already exists (skipped)"
    }

    if (-not $isSuccess) {
        Write-Err "Publication failed!"
        Write-Host ""
        Write-Host $outputString
        Write-Host ""
        Write-Host "Common issues:"
        Write-Host "  - Invalid or expired API key"
        Write-Host "  - Network connection issues"
        Write-Host "  - NuGet.org service temporarily unavailable"
        exit 1
    }

    Write-Host ""
    Write-Success "========================================="
    Write-Success "  Published successfully!"
    Write-Success "========================================="
    Write-Host ""
    Write-Info "View package: https://www.nuget.org/packages/Simple.AutoMapper/"
    Write-Host ""
    Write-Host "Note: It may take a few minutes for the package to appear on NuGet.org"
}

Write-Host ""
Write-Success "Done!"

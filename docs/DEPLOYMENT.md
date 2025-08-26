# Deployment Guide

## Build & Package Using Scripts

The project includes PowerShell scripts in the `scripts/` folder for automated NuGet package management:

### Available Scripts
- **publish-nuget.bat** / **publish-nuget.ps1** - Build, test, and publish package to NuGet
- **unlist-nuget.bat** / **unlist-nuget.ps1** - Unlist a specific version from NuGet
- **nuget-config.ps1.example** - Template for API key configuration

### Quick Start

1. **Configure API Key** (one-time setup):
   ```powershell
   # Option 1: Set API Key as an environment variable (pwsh)
   $env:NUGET_API_KEY = "YOUR_API_KEY"

   # Option 2: Create nuget-config.ps1 and store your key
   Set-Location scripts
   Copy-Item nuget-config.ps1.example nuget-config.ps1 -Force
   # Open nuget-config.ps1 and enter your API Key
   ```

2. **Publish Package**:
   ```powershell
   # Run the batch file from the project root
   scripts\publish-nuget.bat

   # Or run directly from PowerShell (pwsh)
   ./scripts/publish-nuget.ps1 -ApiKey $env:NUGET_API_KEY
   ./scripts/publish-nuget.ps1 -SkipTests  # skip tests
   ./scripts/publish-nuget.ps1 -DryRun     # validate without uploading
   ```

3. **Unlist Package Version** (if needed):
   ```powershell
   # Run the batch file and enter the version
   scripts\unlist-nuget.bat

   # Or specify a version explicitly in PowerShell
   ./scripts/unlist-nuget.ps1 -Version 1.0.5
   ```

## Manual Commands (Alternative)

If you prefer manual commands or the scripts don't work in your environment:

```powershell
# Clean, build, test, and package (pwsh)
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test tests/Simple.AutoMapper.Tests.csproj -c Release
dotnet pack src/Simple.AutoMapper.csproj -c Release -o ./.nupkg

# Upload to NuGet
dotnet nuget push ./.nupkg/Simple.AutoMapper.1.0.5.nupkg `
   --source https://api.nuget.org/v3/index.json `
   --api-key $env:NUGET_API_KEY
```

## Publishing Methods

**Recommended**: Use the provided scripts in `scripts/` folder (see above)

**For CI/CD pipelines**:
```powershell
# Recommended: use --skip-duplicate for idempotent re-runs
dotnet nuget push ./.nupkg/Simple.AutoMapper.{VERSION}.nupkg `
   --source https://api.nuget.org/v3/index.json `
   --api-key $env:NUGET_API_KEY `
   --skip-duplicate
```

## Notes

- Package version is taken from the <Version> in `src/Simple.AutoMapper.csproj`.
- The package includes README.md, LICENSE.md, and the icon (set via SDK properties).

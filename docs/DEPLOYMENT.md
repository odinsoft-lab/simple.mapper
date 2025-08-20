# Deployment Guide

### Build & Package Using Scripts

The project includes PowerShell scripts in the `scripts/` folder for automated NuGet package management:

#### Available Scripts
- **publish-nuget.bat** / **publish-nuget.ps1** - Build, test, and publish package to NuGet
- **unlist-nuget.bat** / **unlist-nuget.ps1** - Unlist a specific version from NuGet
- **nuget-config.ps1.example** - Template for API key configuration

#### Quick Start

1. **Configure API Key** (one-time setup):
   ```powershell
   # Option 1: Set environment variable
   $env:NUGET_API_KEY = "YOUR_API_KEY"
   
   # Option 2: Create nuget-config.ps1 from example
   cd scripts
   copy nuget-config.ps1.example nuget-config.ps1
   # Edit nuget-config.ps1 with your API key
   ```

2. **Publish Package**:
   ```powershell
   # From project root, run the batch file
   scripts\publish-nuget.bat
   
   # Or use PowerShell directly with options
   scripts\publish-nuget.ps1 -ApiKey YOUR_KEY
   scripts\publish-nuget.ps1 -SkipTests  # Skip test execution
   scripts\publish-nuget.ps1 -DryRun     # Test without publishing
   ```

3. **Unlist Package Version** (if needed):
   ```powershell
   # Run the batch file and enter version when prompted
   scripts\unlist-nuget.bat
   
   # Or specify version directly
   scripts\unlist-nuget.ps1 -Version 2.1.0
   ```

#### Manual Commands (Alternative)

If you prefer manual commands or the scripts don't work in your environment:

```bash
# Clean, build, test, and package
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test tests/Mapper.Tests/Mapper.Tests.csproj -c Release
dotnet pack src/OdinMapper/OdinMapper.csproj -c Release 

# Upload to NuGet
dotnet nuget push ./nupkg/OdinMapper.1.0.3.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key YOUR_API_KEY
```

### Publishing Methods

**Recommended**: Use the provided scripts in `scripts/` folder (see above)

**For CI/CD pipelines**:
```bash
# With skip-duplicate flag to avoid errors on re-runs
dotnet nuget push ./nupkg/OdinMapper.{VERSION}.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key $NUGET_API_KEY \
  --skip-duplicate
```

# NuGet Deployment Guide

Step-by-step procedure for releasing a new version of Simple.AutoMapper to NuGet.

---

## Release Checklist

### 1. Update Version

Edit `src/Simple.Mapper.csproj`:

```xml
<Version>1.0.10</Version>
<AssemblyVersion>1.0.10.0</AssemblyVersion>
<FileVersion>1.0.10.0</FileVersion>
```

Update `<PackageReleaseNotes>` in the same file if present.

### 2. Update Release Notes

Add the new version entry to `docs/RELEASE.md`.

### 3. Update README

Update the version badge and release note reference in `README.md` if needed.

### 4. Build & Test

```powershell
dotnet build src/Simple.Mapper.csproj --configuration Release
dotnet test tests/Mapper.Tests.csproj --configuration Release
```

### 5. Create NuGet Package

```powershell
# Dry run (validate without uploading)
powershell -ExecutionPolicy Bypass -File scripts/publish-nuget.ps1 -DryRun

# Or manual pack
dotnet pack src/Simple.Mapper.csproj -c Release -o ./.nupkg
```

### 6. Publish to NuGet

```powershell
# Using script
powershell -ExecutionPolicy Bypass -File scripts/publish-nuget.ps1 -ApiKey "YOUR_API_KEY"

# Or manual upload
dotnet nuget push ./.nupkg/Simple.AutoMapper.1.0.10.nupkg `
    --source https://api.nuget.org/v3/index.json `
    --api-key $env:NUGET_API_KEY
```

### 7. Create GitHub Release

```bash
git tag v1.0.10
git push origin v1.0.10
```

Then create a release on GitHub with the tag, attaching the release notes.

### 8. Verify

- Check https://www.nuget.org/packages/Simple.AutoMapper/ for the new version
- NuGet indexing may take a few minutes

---

## API Key Setup

```powershell
# Option 1: Environment variable
$env:NUGET_API_KEY = "YOUR_API_KEY"

# Option 2: Config file
Copy-Item scripts/nuget-config.ps1.example scripts/nuget-config.ps1
# Edit scripts/nuget-config.ps1 with your key
```

---

## Unlist a Version (if needed)

```powershell
powershell -ExecutionPolicy Bypass -File scripts/unlist-nuget.ps1 -Version 1.0.9
```

---

## Notes

- Package version is taken from `<Version>` in `src/Simple.Mapper.csproj`
- The package includes README.md, LICENSE.md, and the icon
- Use `--skip-duplicate` flag in CI/CD for idempotent re-runs

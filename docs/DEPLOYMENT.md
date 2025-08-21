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
   # Option 1: 환경 변수로 API Key 설정 (pwsh)
   $env:NUGET_API_KEY = "YOUR_API_KEY"

   # Option 2: nuget-config.ps1 생성 후 키 저장
   Set-Location scripts
   Copy-Item nuget-config.ps1.example nuget-config.ps1 -Force
   # nuget-config.ps1 파일을 열어 API Key를 입력하세요
   ```

2. **Publish Package**:
   ```powershell
   # 프로젝트 루트에서 배치 파일 실행
   scripts\publish-nuget.bat

   # 또는 PowerShell에서 직접 실행 (pwsh)
   ./scripts/publish-nuget.ps1 -ApiKey $env:NUGET_API_KEY
   ./scripts/publish-nuget.ps1 -SkipTests  # 테스트 생략
   ./scripts/publish-nuget.ps1 -DryRun     # 업로드 없이 점검
   ```

3. **Unlist Package Version** (if needed):
   ```powershell
   # 배치 파일 실행 후 버전 입력
   scripts\unlist-nuget.bat

   # 또는 PowerShell에서 명시적으로 버전 지정
   ./scripts/unlist-nuget.ps1 -Version 1.0.4
   ```

## Manual Commands (Alternative)

If you prefer manual commands or the scripts don't work in your environment:

```powershell
# Clean, build, test, and package (pwsh)
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test tests/Mapper.Tests.csproj -c Release
dotnet pack src/Simple.AutoMapper.csproj -c Release -o ./.nupkg

# Upload to NuGet
dotnet nuget push ./.nupkg/Simple.AutoMapper.1.0.4.nupkg `
   --source https://api.nuget.org/v3/index.json `
   --api-key $env:NUGET_API_KEY
```

## Publishing Methods

**Recommended**: Use the provided scripts in `scripts/` folder (see above)

**For CI/CD pipelines**:
```powershell
# Re-run 안전성을 위한 skip-duplicate 사용 권장
dotnet nuget push ./.nupkg/Simple.AutoMapper.{VERSION}.nupkg `
   --source https://api.nuget.org/v3/index.json `
   --api-key $env:NUGET_API_KEY `
   --skip-duplicate
```

## Notes

- 패키지 버전은 `src/Simple.AutoMapper.csproj`의 <Version>을 기준으로 합니다.
- 패키지에는 README.md, LICENSE.md, 아이콘(SDK 속성에 설정)이 포함됩니다.

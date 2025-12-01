# Plan: .NET NuGet Package Repository Setup

## Overview
Set up a complete .NET 8 NuGet package repository with central package management, StyleCop code analysis, semantic versioning via Nerdbank.GitVersioning, and xUnit testing infrastructure.

## Requirements Summary
- Single solution file at root
- Directory structure: `library/src/` for libraries, `library/test/` for tests
- First project: Jerry.Library.Grpc
- Each project has README.md
- .NET 8 target framework
- Directory.Build.props and Directory.Packages.props at root
- StyleCop with default ruleset
- xUnit for testing
- Nerdbank.GitVersioning with semantic versioning (major.minor.patch-prerelease+buildmetadata)

## Directory Structure

```
/Users/markjungeblut/development/jerry-nuget/
├── .gitignore
├── Jerry.sln
├── version.json
├── Directory.Build.props
├── Directory.Packages.props
├── stylecop.json
├── README.md
├── library/
│   ├── src/
│   │   └── Jerry.Library.Grpc/
│   │       ├── Jerry.Library.Grpc.csproj
│   │       └── README.md
│   └── test/
│       └── Jerry.Library.Grpc.Tests/
│           ├── Jerry.Library.Grpc.Tests.csproj
│           └── README.md
```

## Implementation Steps

### Phase 0: Setup Plan Storage

#### 0. Create Repository Plans Folder and Copy This Plan

```bash
mkdir -p /Users/markjungeblut/development/jerry-nuget/.claude/plans
cp /Users/markjungeblut/.claude/plans/synthetic-hopping-clarke.md /Users/markjungeblut/development/jerry-nuget/.claude/plans/nuget-repository-setup.md
```

**Purpose**: Store this implementation plan within the repository for version control and team reference.

### Phase 1: Foundation Files

#### 1. Create .gitignore
```bash
dotnet new gitignore
```

#### 2. Create Directory Structure
```bash
mkdir -p library/src/Jerry.Library.Grpc
mkdir -p library/test/Jerry.Library.Grpc.Tests
```

#### 3. Create version.json

**Purpose**: Configure Nerdbank.GitVersioning for semantic versioning with git height

**Content**:
```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/master/src/NerdBank.GitVersioning/version.schema.json",
  "version": "0.1.0-alpha",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/heads/master$",
    "^refs/tags/v\\d+\\.\\d+\\.\\d+$"
  ],
  "nugetPackageVersion": {
    "semVer": 2
  },
  "cloudBuild": {
    "buildNumber": {
      "enabled": true,
      "includeCommitId": {
        "when": "nonPublicReleaseOnly",
        "where": "buildMetadata"
      }
    }
  }
}
```

**Versioning Behavior**:
- Development builds: `0.1.0-alpha.45+g1234abcd` (45 = git height, g1234abcd = commit SHA)
- Tagged releases: `0.1.0` (clean version)
- Use `nbgv set-version X.Y.Z` to update major/minor/patch versions

#### 4. Create Directory.Packages.props

**Purpose**: Central package management for all NuGet dependencies

**Content**:
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup Label="Build and Versioning">
    <PackageVersion Include="Nerdbank.GitVersioning" Version="3.6.143">
      <PrivateAssets>all</PrivateAssets>
    </PackageVersion>
    <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
    </PackageVersion>
  </ItemGroup>

  <ItemGroup Label="Code Analysis">
    <PackageVersion Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageVersion>
  </ItemGroup>

  <ItemGroup Label="Testing">
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageVersion>
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageVersion Include="coverlet.collector" Version="6.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageVersion>
  </ItemGroup>

  <ItemGroup Label="gRPC Dependencies">
    <PackageVersion Include="Grpc.AspNetCore" Version="2.66.0" />
    <PackageVersion Include="Grpc.Tools" Version="2.66.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageVersion>
    <PackageVersion Include="Google.Protobuf" Version="3.28.3" />
  </ItemGroup>
</Project>
```

#### 5. Create Directory.Build.props

**Purpose**: Common MSBuild properties for all projects

**Content**:
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>

  <PropertyGroup>
    <Authors>Your Name or Organization</Authors>
    <Company>Your Company</Company>
    <Copyright>Copyright © $(Company) $([System.DateTime]::Now.Year)</Copyright>
    <PackageProjectUrl>https://github.com/yourorg/jerry-nuget</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourorg/jerry-nuget.git</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>
  </PropertyGroup>

  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);CS1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)stylecop.json" Link="stylecop.json" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="All" />
  </ItemGroup>
</Project>
```

**Note**: Update Authors, Company, PackageProjectUrl, RepositoryUrl, and PackageLicenseExpression as needed.

#### 6. Create stylecop.json

**Purpose**: StyleCop analyzer configuration

**Content**:
```json
{
  "$schema": "https://raw.githubusercontent.com/DotNetAnalyzers/StyleCopAnalyzers/master/StyleCop.Analyzers/StyleCop.Analyzers/Settings/stylecop.schema.json",
  "settings": {
    "documentationRules": {
      "companyName": "Your Company",
      "copyrightText": "Copyright (c) {companyName}. All rights reserved.",
      "xmlHeader": true,
      "fileNamingConvention": "stylecop"
    },
    "orderingRules": {
      "usingDirectivesPlacement": "outsideNamespace",
      "systemUsingDirectivesFirst": true
    },
    "namingRules": {
      "allowCommonHungarianPrefixes": false,
      "allowedHungarianPrefixes": []
    },
    "maintainabilityRules": {
      "topLevelTypes": [
        "class",
        "interface",
        "struct"
      ]
    },
    "layoutRules": {
      "newlineAtEndOfFile": "require"
    }
  }
}
```

### Phase 2: Solution and Projects

#### 7. Create Solution File
```bash
dotnet new sln -n Jerry
```

#### 8. Create Library Project

```bash
dotnet new classlib -n Jerry.Library.Grpc -o library/src/Jerry.Library.Grpc
```

**Then replace the generated .csproj with**:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <PackageId>Jerry.Library.Grpc</PackageId>
    <Description>A gRPC library for Jerry services</Description>
    <PackageTags>grpc;library;jerry</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Grpc.AspNetCore" />
    <PackageReference Include="Google.Protobuf" />
    <PackageReference Include="Grpc.Tools" />
    <PackageReference Include="StyleCop.Analyzers" />
  </ItemGroup>

</Project>
```

**Note**: Package versions omitted - managed centrally in Directory.Packages.props

#### 9. Create Test Project

```bash
dotnet new xunit -n Jerry.Library.Grpc.Tests -o library/test/Jerry.Library.Grpc.Tests
```

**Then replace the generated .csproj with**:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="coverlet.collector" />
    <PackageReference Include="StyleCop.Analyzers" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Jerry.Library.Grpc\Jerry.Library.Grpc.csproj" />
  </ItemGroup>

</Project>
```

#### 10. Add Projects to Solution

```bash
dotnet sln add library/src/Jerry.Library.Grpc/Jerry.Library.Grpc.csproj
dotnet sln add library/test/Jerry.Library.Grpc.Tests/Jerry.Library.Grpc.Tests.csproj
```

### Phase 3: Documentation

#### 11. Create Root README.md

```markdown
# Jerry NuGet Package Repository

A collection of reusable .NET libraries for Jerry services.

## Projects

- **Jerry.Library.Grpc** - gRPC library for service communication

## Build Requirements

- .NET 8.0 SDK or later
- Git (for Nerdbank.GitVersioning)

## Building

```bash
dotnet restore
dotnet build
```

## Testing

```bash
dotnet test
```

## Packaging

Packages are automatically generated during build. Find them in:
```
library/src/[ProjectName]/bin/[Configuration]/[ProjectName].[Version].nupkg
```

## Versioning

This repository uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for semantic versioning.

### Version Format
- Development: `1.2.3-alpha.45+g1234abcd` (git height + commit SHA)
- Release: `1.2.3` (clean version on tagged commits)

### Updating Versions
Use the nbgv tool to set major/minor/patch versions:
```bash
dotnet tool install -g nbgv
nbgv set-version 1.2.3
```

## Code Quality

- **StyleCop.Analyzers** - Enforces C# style and consistency rules
- **Central Package Management** - All package versions managed in Directory.Packages.props

## Contributing

1. Follow the existing project structure
2. Ensure all tests pass
3. Follow StyleCop guidelines (warnings treated as errors)
```

#### 12. Create Library README.md

**Location**: `library/src/Jerry.Library.Grpc/README.md`

```markdown
# Jerry.Library.Grpc

gRPC library providing common functionality for Jerry services.

## Installation

```bash
dotnet add package Jerry.Library.Grpc
```

## Usage

[To be added as implementation progresses]

## Dependencies

- Grpc.AspNetCore
- Google.Protobuf

## Development

This project follows standard .NET development practices with StyleCop enforcement.

## Testing

Tests are located in `library/test/Jerry.Library.Grpc.Tests/`

```bash
dotnet test
```
```

#### 13. Create Test README.md

**Location**: `library/test/Jerry.Library.Grpc.Tests/README.md`

```markdown
# Jerry.Library.Grpc.Tests

Unit tests for Jerry.Library.Grpc library.

## Running Tests

From repository root:
```bash
dotnet test library/test/Jerry.Library.Grpc.Tests
```

## Test Framework

- xUnit 2.9.2
- Microsoft.NET.Test.Sdk

## Coverage

Tests use coverlet.collector for code coverage analysis.
```

### Phase 4: Verification

#### 14. Initialize Git Repository

```bash
git init
git add .
git commit -m "Initial repository setup with Nerdbank.GitVersioning"
```

**Note**: Git repository required for Nerdbank.GitVersioning to work

#### 15. Restore and Build

```bash
dotnet restore
dotnet build
```

**Expected Output**:
- Build succeeds
- NuGet packages generated in `library/src/Jerry.Library.Grpc/bin/Debug/`
- Version number includes git height (e.g., `0.1.0-alpha.1.nupkg`)

#### 16. Run Tests

```bash
dotnet test
```

**Expected**: All tests pass (default xUnit test should pass)

#### 17. Verify Versioning

```bash
dotnet build --verbosity normal
```

Look for version information in build output showing semantic version with git height.

## Key Configuration Points

### Semantic Versioning Strategy
- **Manual control**: Use `nbgv set-version X.Y.Z` to update major/minor/patch
- **Automatic build numbers**: Git height automatically appended during development
- **Pre-release tags**: Non-main/master branches get `-alpha`, `-beta`, etc.
- **Release process**: Tag commits (e.g., `v1.2.3`) to produce clean versions

### Central Package Management Benefits
- Single source of truth for package versions
- Easier to update dependencies across all projects
- Prevents version conflicts
- Organized by category (Build, Testing, gRPC, etc.)

### StyleCop Enforcement
- Runs during build
- Warnings treated as errors
- Configuration in `stylecop.json`
- Consistent code style across all projects

### NuGet Package Configuration
- `GeneratePackageOnBuild=true` creates packages on every build
- README.md included in packages
- Symbol packages (.snupkg) for debugging
- SourceLink for source debugging

## Future Enhancements

1. **Pre-commit Hook**: Add git pre-commit hook for `nbgv set-version` workflow
2. **CI/CD Pipeline**: GitHub Actions or Azure Pipelines for automated builds/releases
3. **Additional Libraries**: Follow same pattern for new projects
4. **Release Automation**: Automate tagging and NuGet publishing

## Critical Files Created

1. `/Users/markjungeblut/development/jerry-nuget/.gitignore`
2. `/Users/markjungeblut/development/jerry-nuget/version.json`
3. `/Users/markjungeblut/development/jerry-nuget/Directory.Build.props`
4. `/Users/markjungeblut/development/jerry-nuget/Directory.Packages.props`
5. `/Users/markjungeblut/development/jerry-nuget/stylecop.json`
6. `/Users/markjungeblut/development/jerry-nuget/Jerry.sln`
7. `/Users/markjungeblut/development/jerry-nuget/README.md`
8. `/Users/markjungeblut/development/jerry-nuget/library/src/Jerry.Library.Grpc/Jerry.Library.Grpc.csproj`
9. `/Users/markjungeblut/development/jerry-nuget/library/src/Jerry.Library.Grpc/README.md`
10. `/Users/markjungeblut/development/jerry-nuget/library/test/Jerry.Library.Grpc.Tests/Jerry.Library.Grpc.Tests.csproj`
11. `/Users/markjungeblut/development/jerry-nuget/library/test/Jerry.Library.Grpc.Tests/README.md`

## Success Criteria

- ✓ Solution builds without errors
- ✓ All tests pass
- ✓ NuGet packages generated with semantic versions
- ✓ StyleCop rules enforced
- ✓ Central package management working
- ✓ Git-based versioning operational
- ✓ Documentation complete

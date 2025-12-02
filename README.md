# Jerry NuGet Package Repository

A collection of reusable .NET libraries for Jerry services.

## Projects

- **Jerry.Library.Grpc** - gRPC library for service communication (v8.0.0-alpha)
- **Jerry.Library.Telemetry** - Telemetry and observability library (v8.0.0-alpha)

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

This repository uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for **per-project semantic versioning**.

### Per-Project Versioning (Automatic)
- Each library has its own `version.json` file in its project directory
- **Automatic**: Version only increments when commits modify files in that specific library's directory
- The `pathFilters: ["."]` setting in each version.json ensures independent tracking
- No manual intervention needed - just commit changes and build
- This allows independent release cycles for each library

#### How It Works
1. You modify a file in `library/src/Jerry.Library.Grpc/`
2. Commit the change
3. Build → Only Grpc version increments (e.g., `8.0.0-alpha` → `8.0.0-alpha.1`)
4. Telemetry version stays unchanged (still `8.0.0-alpha`)

### Version Format
- **Main branch**: `8.0.0-alpha.45+g1234abcd` (always prerelease with git height + commit SHA)
- **Release branch**: `8.0.0` (clean version on `release/*` branches when tagged)
- Git height only counts commits that touch the library's directory

### Release Branch Strategy
This repository uses per-library release branches for production releases:
- `main` branch → Always produces prerelease versions (e.g., `8.0.0-alpha.1`)
- `release/{library}/{version}` branches → Produce clean releases when tagged (e.g., `release/grpc/8.1`)
- Examples: `release/grpc/8.1`, `release/telemetry/8.0`
- Benefits:
  - Safer release process - stabilize code before release
  - Main branch continues with new development
  - Support hotfixes on release branches
  - Prevents accidental releases from main
  - Independent release cycles per library

### Versioning Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                    Development Workflow                         │
└─────────────────────────────────────────────────────────────────┘

  Make changes to library          Commit changes              Build project
  ┌──────────────────┐            ┌──────────────┐            ┌──────────────┐
  │ Edit files in:   │            │ git add .    │            │ dotnet build │
  │ library/src/     │  ────────> │ git commit   │  ────────> │              │
  │ Jerry.Library.*/ │            │              │            │              │
  └──────────────────┘            └──────────────┘            └──────────────┘
                                                                      │
                                                                      ▼
  ┌─────────────────────────────────────────────────────────────────────────┐
  │  Nerdbank.GitVersioning automatically increments version:               │
  │  • Counts commits touching the library directory (git height)           │
  │  • Applies pathFilters to track only relevant changes                   │
  │  • Example: 8.0.0-alpha → 8.0.0-alpha.1 → 8.0.0-alpha.2                │
  └─────────────────────────────────────────────────────────────────────────┘
                                          │
                                          ▼
  ┌─────────────────────────────────────────────────────────────────┐
  │                    Release Workflow                             │
  └─────────────────────────────────────────────────────────────────┘

  Create release branch            Tag on release branch       Build & publish
  ┌──────────────────────┐            ┌──────────────┐            ┌──────────────┐
  │ git checkout -b      │            │ git tag      │            │ dotnet build │
  │ release/grpc/8.1     │  ────────> │ v8.1.0       │  ────────> │ dotnet pack  │
  │ git push -u origin   │            │ git push     │            │              │
  │ release/grpc/8.1     │            │ --tags       │            │              │
  └──────────────────────┘            └──────────────┘            └──────────────┘
                                                                      │
                                                                      ▼
  ┌─────────────────────────────────────────────────────────────────────────┐
  │  Clean release version generated on release branch:                     │
  │  • 8.1.0 (no prerelease suffix on release/{library}/* branches)         │
  │  • NuGet package: Jerry.Library.Grpc.8.1.0.nupkg                        │
  │  • Main branch continues to produce prerelease versions                 │
  └─────────────────────────────────────────────────────────────────────────┘
```
```

### Creating a Release

To create a new release:

```bash
# 1. Update version in the library directory (on main branch)
cd library/src/Jerry.Library.Grpc
nbgv set-version 8.1.0
git add version.json
git commit -m "Bump Jerry.Library.Grpc version to 8.1.0"
git push

# 2. Create and push release branch (use library name in branch)
git checkout -b release/grpc/8.1
git push -u origin release/grpc/8.1

# 3. Tag the release on the release branch
git tag v8.1.0
git push --tags

# 4. Build and pack
dotnet build -c Release
dotnet pack -c Release
```

The release branch naming convention `release/{library}/{version}`:
- `grpc` for Jerry.Library.Grpc
- `telemetry` for Jerry.Library.Telemetry
- Allows independent releases per library
- Can be used for stabilization, hotfixes, and patch releases

### Updating Version Numbers
```bash
# Install nbgv tool if needed
dotnet tool install -g nbgv

# Update version for a library
cd library/src/Jerry.Library.Grpc
nbgv set-version 8.2.0
```

## Code Quality

- **.editorconfig** - Enforces C# style and consistency rules via IDE and build
- **Central Package Management** - All package versions managed in Directory.Packages.props
- **EnforceCodeStyleInBuild** - Code style violations treated as build errors

## Contributing

1. Follow the existing project structure
2. Ensure all tests pass
3. Follow .editorconfig code style guidelines (enforced during build)

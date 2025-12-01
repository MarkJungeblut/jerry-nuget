# Jerry NuGet Package Repository

A collection of reusable .NET libraries for Jerry services.

## Projects

- **Jerry.Library.Grpc** - gRPC library for service communication (v8.0.0-alpha)
- **Jerry.Library.Telemetry** - Telemetry and observability library (v8.0.0-beta)

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
4. Telemetry version stays unchanged (still `8.0.0-beta`)

### Version Format
- Development: `1.2.3-alpha.45+g1234abcd` (git height + commit SHA)
- Release: `1.2.3` (clean version on tagged commits)
- Git height only counts commits that touch the library's directory

### Updating Versions
Navigate to the library directory and update its version.json:
```bash
cd library/src/Jerry.Library.Grpc
# Edit version.json to change the version
# OR use nbgv tool:
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

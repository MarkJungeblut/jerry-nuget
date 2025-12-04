# Jerry.Library.Grpc

gRPC library providing common functionality for Jerry services.

## Installation

```bash
dotnet add package Jerry.Library.Grpc
```

## Features

- [Interceptors](Interceptors/README.md) - Server interceptors for exception logging and other cross-cutting concerns

## Dependencies

- Grpc.AspNetCore
- Google.Protobuf
- Microsoft.Extensions.Logging

## Development

This project follows standard .NET development practices with code style enforcement via .editorconfig.

## Testing

Tests are located in `library/test/Jerry.Library.Grpc.Tests/`

```bash
dotnet test
```

# Jerry.Library.Grpc

gRPC library providing common functionality for Jerry services.

## Installation

```bash
dotnet add package Jerry.Library.Grpc
```

## Features

### ExceptionLoggingInterceptor

A gRPC server interceptor that automatically logs exceptions and converts them to appropriate gRPC status codes.

#### Exception Mapping

The interceptor automatically maps .NET exceptions to gRPC status codes:

| .NET Exception | gRPC Status Code | Log Level |
|---|---|---|
| `RpcException` | (preserved) | Warning |
| `ArgumentException` | `InvalidArgument` | Warning |
| `InvalidOperationException` | `FailedPrecondition` | Warning |
| Any other exception | `Internal` | Error |

#### Usage

Register the interceptor in your ASP.NET Core application:

```csharp
using Jerry.Library.Grpc.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Register the interceptor
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionLoggingInterceptor>();
});

var app = builder.Build();
app.MapGrpcService<MyGrpcService>();
app.Run();
```

#### Benefits

- **Centralized Exception Handling**: Catch and log all exceptions in one place
- **Consistent Error Responses**: Automatically convert exceptions to appropriate gRPC status codes
- **Detailed Logging**: Logs include method name, status code, and exception details
- **Security**: Prevents internal exception details from leaking to clients (converts to generic "Internal error" message)

#### Example Log Output

```
[Warning] gRPC call to /MyService/GetUser failed with status InvalidArgument: User ID cannot be empty
[Error] Unhandled exception in gRPC call to /MyService/ProcessOrder
```

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

The test suite includes comprehensive tests for the `ExceptionLoggingInterceptor` using NSubstitute for mocking.

# gRPC Channel Fixture Implementation Plan

## Overview

Create a reusable xUnit fixture (`GrpcServerFixture`) to eliminate boilerplate code for gRPC testing in the Jerry.Library.Grpc.Tests project. This fixture will provide a simple API for starting gRPC servers with custom services, interceptors, and mock dependencies.

## Current Problem

The existing `ExceptionLoggingInterceptorTests` class contains ~70 lines of duplicated setup code:
- AppContext switch for HTTP/2 unencrypted support
- WebApplication builder configuration
- Kestrel HTTP/2 setup with dynamic port allocation
- Service and interceptor registration
- Channel creation and client instantiation
- Manual IDisposable cleanup

Every new test class must duplicate this pattern, increasing maintenance burden and risk of configuration errors.

## Recommended Solution: Simple Class-Level Fixture

Implement `GrpcServerFixture` using xUnit's `IClassFixture<T>` pattern with:
- **Simple fluent API** for configuration before server starts
- **Lazy initialization** to allow complete configuration before startup
- **Generic client creation** for type-safe client instantiation
- **Automatic cleanup** via `IAsyncDisposable`
- **Test isolation** with one server instance per test class (per user preference)

## Implementation Steps

### Step 1: Create Core Fixture Class

**File**: `library/test/Jerry.Library.Grpc.Tests/Fixtures/GrpcServerFixture.cs`

Key components:
- Static constructor to set HTTP/2 AppContext switch
- Simple configuration methods: `ConfigureServices()`, `ConfigureGrpc()`, `AddGrpcService<T>()`
- Lazy server initialization on first access to `Channel` or `CreateClient<T>()`
- Generic `CreateClient<TClient>()` using reflection to instantiate gRPC clients
- `IAsyncDisposable` implementation for proper cleanup

Configuration is locked after server starts (throws exception if modified later).

### Step 2: Refactor ExceptionLoggingInterceptorTests

Reduce from ~70 lines of setup to ~8 lines:

**Before**:
```csharp
public class ExceptionLoggingInterceptorTests : IDisposable
{
    private readonly WebApplication _app;
    private readonly GrpcChannel _channel;
    // ... 60+ lines of setup in constructor
    public void Dispose() { /* cleanup */ }
}
```

**After**:
```csharp
public class ExceptionLoggingInterceptorTests : IClassFixture<GrpcServerFixture>
{
    private readonly TestService.TestServiceClient _client;

    public ExceptionLoggingInterceptorTests(GrpcServerFixture fixture)
    {
        fixture
            .ConfigureServices(services => services.AddSingleton(_mockLogger))
            .ConfigureGrpc(options => options.Interceptors.Add<ExceptionLoggingInterceptor>())
            .AddGrpcService<TestServiceImpl>();
        _client = fixture.CreateClient<TestService.TestServiceClient>();
    }
}
```

### Step 3: Update Documentation

Update `library/test/Jerry.Library.Grpc.Tests/README.md` with:
- Simple fixture usage guide with basic examples
- Migration guide from old pattern to new pattern

## Usage Examples

### Basic Usage
```csharp
public class MyTests : IClassFixture<GrpcServerFixture>
{
    private readonly MyService.MyServiceClient _client;

    public MyTests(GrpcServerFixture fixture)
    {
        fixture.AddGrpcService<MyServiceImpl>();
        _client = fixture.CreateClient<MyService.MyServiceClient>();
    }
}
```

### With Interceptors and Mocks
```csharp
public MyTests(GrpcServerFixture fixture)
{
    var mockLogger = Substitute.For<ILogger<MyInterceptor>>();

    fixture
        .ConfigureServices(s => s.AddSingleton(mockLogger))
        .ConfigureGrpc(o => o.Interceptors.Add<MyInterceptor>())
        .AddGrpcService<MyServiceImpl>();

    _client = fixture.CreateClient<MyService.MyServiceClient>();
}
```

### Multiple Services
```csharp
public MyTests(GrpcServerFixture fixture)
{
    fixture
        .AddGrpcService<UserServiceImpl>()
        .AddGrpcService<OrderServiceImpl>();

    _userClient = fixture.CreateClient<UserService.UserServiceClient>();
    _orderClient = fixture.CreateClient<OrderService.OrderServiceClient>();
}
```

## Technical Details

### HTTP/2 Configuration
- AppContext switch set in static constructor (runs once per AppDomain)
- Kestrel configured with `HttpProtocols.Http2`
- Port 0 for dynamic allocation (prevents port conflicts in parallel execution)

### Lifecycle Management
- One server instance per test class (via `IClassFixture<T>`)
- Lazy initialization: server starts on first `Channel` or `CreateClient<T>()` access
- Thread-safe initialization with lock around server startup
- Async disposal via `IAsyncDisposable` (supported by xUnit 2.9.2)

### Error Handling
Clear exceptions for common mistakes:
- "Configuration cannot be changed after server has started"
- "At least one gRPC service must be registered"
- "Client type {T} does not have a constructor accepting ChannelBase"

### Client Creation
Use reflection-based approach for `CreateClient<T>()`:
```csharp
public TClient CreateClient<TClient>() where TClient : ClientBase<TClient>
{
    var constructor = typeof(TClient).GetConstructor(new[] { typeof(ChannelBase) });
    return (TClient)constructor.Invoke(new object[] { Channel });
}
```

## File Structure

```
library/test/Jerry.Library.Grpc.Tests/
├── Fixtures/
│   └── GrpcServerFixture.cs          (New - core fixture)
├── Interceptors/
│   └── ExceptionLoggingInterceptorTests.cs (Modified - refactored to use fixture)
├── Services/
│   └── TestServiceImpl.cs            (Unchanged)
├── Protos/
│   └── test.proto                    (Unchanged)
├── Jerry.Library.Grpc.Tests.csproj   (Unchanged)
└── README.md                         (Modified - add fixture documentation)
```

## Benefits

1. **Code reduction**: 40-50 lines of boilerplate removed per test class
2. **Consistency**: Standardized setup across all test classes
3. **Maintainability**: Configuration changes in one place (fixture), not scattered across tests
4. **Safety**: Automatic cleanup prevents resource leaks
5. **Developer experience**: New test classes created in minutes, not hours

## Critical Files to Modify

1. `library/test/Jerry.Library.Grpc.Tests/Fixtures/GrpcServerFixture.cs` (create)
2. `library/test/Jerry.Library.Grpc.Tests/Interceptors/ExceptionLoggingInterceptorTests.cs` (refactor)
3. `library/test/Jerry.Library.Grpc.Tests/README.md` (update)

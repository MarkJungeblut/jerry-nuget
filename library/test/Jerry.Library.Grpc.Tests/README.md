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

## gRPC Test Fixture

The `GrpcServerFixture` provides a simple way to test gRPC services without duplicating server setup code. It uses xUnit's `IClassFixture<T>` to provide one server instance per test class.

### Basic Usage

```csharp
public class MyServiceTests : IClassFixture<GrpcServerFixture>
{
    private readonly MyService.MyServiceClient _client;

    public MyServiceTests(GrpcServerFixture fixture)
    {
        fixture.AddGrpcService<MyServiceImpl>();
        _client = fixture.CreateClient<MyService.MyServiceClient>();
    }

    [Fact]
    public void MyTest()
    {
        var response = _client.MyMethod(new MyRequest());
        Assert.NotNull(response);
    }
}
```

### With Interceptors and Mocks

```csharp
public class InterceptorTests : IClassFixture<GrpcServerFixture>
{
    private readonly ILogger<MyInterceptor> _mockLogger;
    private readonly MyService.MyServiceClient _client;

    public InterceptorTests(GrpcServerFixture fixture)
    {
        _mockLogger = Substitute.For<ILogger<MyInterceptor>>();

        fixture
            .ConfigureServices(services => services.AddSingleton(_mockLogger))
            .ConfigureGrpc(options => options.Interceptors.Add<MyInterceptor>())
            .AddGrpcService<MyServiceImpl>();

        _client = fixture.CreateClient<MyService.MyServiceClient>();
    }
}
```

### Multiple Services

```csharp
public class MultiServiceTests : IClassFixture<GrpcServerFixture>
{
    private readonly UserService.UserServiceClient _userClient;
    private readonly OrderService.OrderServiceClient _orderClient;

    public MultiServiceTests(GrpcServerFixture fixture)
    {
        fixture
            .AddGrpcService<UserServiceImpl>()
            .AddGrpcService<OrderServiceImpl>();

        _userClient = fixture.CreateClient<UserService.UserServiceClient>();
        _orderClient = fixture.CreateClient<OrderService.OrderServiceClient>();
    }
}
```

### Features

- **Automatic server lifecycle**: Server starts lazily on first client access
- **HTTP/2 support**: Configured automatically for gRPC testing
- **Dynamic port allocation**: Prevents port conflicts when running tests in parallel
- **Fluent API**: Chain configuration methods for clean test setup
- **Automatic cleanup**: Implements `IDisposable` for proper resource management

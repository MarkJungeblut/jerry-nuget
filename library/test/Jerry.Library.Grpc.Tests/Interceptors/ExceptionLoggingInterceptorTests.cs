// <copyright file="ExceptionLoggingInterceptorTests.cs" company="Your Company">
// Copyright (c) Your Company. All rights reserved.
// </copyright>

namespace Jerry.Library.Grpc.Tests.Interceptors;

using System.Net.Http;
using global::Grpc.Core;
using global::Grpc.Net.Client;
using Jerry.Library.Grpc.Interceptors;
using Jerry.Library.Grpc.Tests.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests for <see cref="ExceptionLoggingInterceptor"/>.
/// </summary>
public class ExceptionLoggingInterceptorTests : IDisposable
{
    private readonly ILogger<ExceptionLoggingInterceptor> _mockLogger;
    private readonly WebApplication _app;
    private readonly TestService.TestServiceClient _client;
    private readonly GrpcChannel _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionLoggingInterceptorTests"/> class.
    /// </summary>
    public ExceptionLoggingInterceptorTests()
    {
        // Enable HTTP/2 without TLS for testing
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        _mockLogger = Substitute.For<ILogger<ExceptionLoggingInterceptor>>();

        var builder = WebApplication.CreateBuilder();

        // Configure Kestrel to use HTTP/2 without TLS
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Listen(System.Net.IPAddress.Loopback, 0, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        });

        // Disable logging noise in tests
        builder.Logging.ClearProviders();

        // Register the interceptor with the mock logger
        builder.Services.AddSingleton(_mockLogger);
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionLoggingInterceptor>();
        });

        _app = builder.Build();
        _app.MapGrpcService<TestServiceImpl>();

        _app.StartAsync().Wait();

        // Get the actual port and create channel
        var address = _app.Urls.First();
        _channel = GrpcChannel.ForAddress(address);
        _client = new TestService.TestServiceClient(_channel);
    }

    /// <summary>
    /// Tests that successful calls are not logged and return the expected response.
    /// </summary>
    [Fact]
    public void Echo_SuccessfulCall_ReturnsResponse()
    {
        // Arrange
        var request = new EchoRequest { Message = "test" };

        // Act
        var response = _client.Echo(request);

        // Assert
        Assert.Equal("test", response.Message);
        _mockLogger.DidNotReceive().Log(
            Arg.Any<LogLevel>(),
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that RpcException is logged as warning and re-thrown.
    /// </summary>
    [Fact]
    public void ThrowException_RpcException_LogsWarningAndRethrows()
    {
        // Arrange
        var request = new ExceptionRequest
        {
            ExceptionType = "RpcException",
            Message = "Resource not found",
        };

        // Act & Assert
        var exception = Assert.Throws<RpcException>(() => _client.ThrowException(request));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Equal("Resource not found", exception.Status.Detail);
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<RpcException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that ArgumentException is logged as warning and converted to InvalidArgument status.
    /// </summary>
    [Fact]
    public void ThrowException_ArgumentException_LogsWarningAndConvertsToInvalidArgument()
    {
        // Arrange
        var request = new ExceptionRequest
        {
            ExceptionType = "ArgumentException",
            Message = "Invalid parameter",
        };

        // Act & Assert
        var exception = Assert.Throws<RpcException>(() => _client.ThrowException(request));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        Assert.Equal("Invalid parameter", exception.Status.Detail);
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<ArgumentException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that InvalidOperationException is logged as warning and converted to FailedPrecondition status.
    /// </summary>
    [Fact]
    public void ThrowException_InvalidOperationException_LogsWarningAndConvertsToFailedPrecondition()
    {
        // Arrange
        var request = new ExceptionRequest
        {
            ExceptionType = "InvalidOperationException",
            Message = "Operation not allowed",
        };

        // Act & Assert
        var exception = Assert.Throws<RpcException>(() => _client.ThrowException(request));

        Assert.Equal(StatusCode.FailedPrecondition, exception.StatusCode);
        Assert.Equal("Operation not allowed", exception.Status.Detail);
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<InvalidOperationException>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that generic Exception is logged as error and converted to Internal status.
    /// </summary>
    [Fact]
    public void ThrowException_UnhandledException_LogsErrorAndConvertsToInternal()
    {
        // Arrange
        var request = new ExceptionRequest
        {
            ExceptionType = "UnknownException",
            Message = "Unexpected error",
        };

        // Act & Assert
        var exception = Assert.Throws<RpcException>(() => _client.ThrowException(request));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
        Assert.Equal("An internal error occurred", exception.Status.Detail);
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Tests that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ExceptionLoggingInterceptor(null!));
    }

    /// <summary>
    /// Disposes the test resources.
    /// </summary>
    public void Dispose()
    {
        _channel?.Dispose();
        _app?.StopAsync().Wait();
        _app?.DisposeAsync().AsTask().Wait();
    }
}

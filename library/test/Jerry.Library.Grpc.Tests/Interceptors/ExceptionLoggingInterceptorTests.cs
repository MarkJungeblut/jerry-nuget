// <copyright file="ExceptionLoggingInterceptorTests.cs" company="Your Company">
// Copyright (c) Your Company. All rights reserved.
// </copyright>

namespace Jerry.Library.Grpc.Tests.Interceptors;

using global::Grpc.Core;
using Jerry.Library.Grpc.Interceptors;
using Jerry.Library.Grpc.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// Tests for <see cref="ExceptionLoggingInterceptor"/>.
/// </summary>
public class ExceptionLoggingInterceptorTests : IClassFixture<ExceptionLoggingTestFixture>
{
    private readonly ILogger<ExceptionLoggingInterceptor> _mockLogger;
    private readonly TestService.TestServiceClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionLoggingInterceptorTests"/> class.
    /// </summary>
    /// <param name="fixture">The gRPC server fixture with exception logging configured.</param>
    public ExceptionLoggingInterceptorTests(ExceptionLoggingTestFixture fixture)
    {
        _mockLogger = fixture.MockLogger;
        _client = fixture.CreateClient<TestService.TestServiceClient>();
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
}

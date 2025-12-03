// <copyright file="ExceptionLoggingInterceptor.cs" company="Your Company">
// Copyright (c) Your Company. All rights reserved.
// </copyright>

namespace Jerry.Library.Grpc.Interceptors;

using global::Grpc.Core;
using global::Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

/// <summary>
/// gRPC server interceptor that automatically logs exceptions and converts them to appropriate gRPC status codes.
/// </summary>
/// <remarks>
/// <para>
/// This interceptor provides centralized exception handling for all gRPC services in your application.
/// It catches exceptions thrown by service methods (unary, server streaming, client streaming, and duplex streaming),
/// logs them with appropriate log levels, and converts them to gRPC status codes that clients can handle.
/// </para>
/// <para><strong>Exception Mapping:</strong></para>
/// <list type="table">
/// <listheader>
/// <term>.NET Exception</term>
/// <term>gRPC Status Code</term>
/// <term>Log Level</term>
/// </listheader>
/// <item>
/// <term><see cref="RpcException"/></term>
/// <term>(preserved)</term>
/// <term>Warning</term>
/// </item>
/// <item>
/// <term><see cref="ArgumentException"/></term>
/// <term>InvalidArgument</term>
/// <term>Warning</term>
/// </item>
/// <item>
/// <term><see cref="InvalidOperationException"/></term>
/// <term>FailedPrecondition</term>
/// <term>Warning</term>
/// </item>
/// <item>
/// <term>Any other exception</term>
/// <term>Internal</term>
/// <term>Error</term>
/// </item>
/// </list>
/// <para><strong>Usage:</strong></para>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
///
/// // Register the interceptor
/// builder.Services.AddGrpc(options =>
/// {
///     options.Interceptors.Add&lt;ExceptionLoggingInterceptor&gt;();
/// });
///
/// var app = builder.Build();
/// app.MapGrpcService&lt;MyGrpcService&gt;();
/// app.Run();
/// </code>
/// <para><strong>Benefits:</strong></para>
/// <list type="bullet">
/// <item><description>Centralized exception handling - catch and log all exceptions in one place</description></item>
/// <item><description>Consistent error responses - automatically convert exceptions to appropriate gRPC status codes</description></item>
/// <item><description>Detailed logging - logs include method name, status code, and exception details</description></item>
/// <item><description>Security - prevents internal exception details from leaking to clients (converts to generic "Internal error" message)</description></item>
/// </list>
/// </remarks>
/// <example>
/// Log output examples:
/// <code>
/// [Warning] gRPC call to /MyService/GetUser failed with status InvalidArgument: User ID cannot be empty
/// [Error] Unhandled exception in gRPC call to /MyService/ProcessOrder
/// </code>
/// </example>
public class ExceptionLoggingInterceptor : Interceptor
{
    private readonly ILogger<ExceptionLoggingInterceptor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionLoggingInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ExceptionLoggingInterceptor(ILogger<ExceptionLoggingInterceptor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Intercepts unary server calls to log exceptions.
    /// </summary>
    /// <typeparam name="TRequest">The request message type.</typeparam>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="request">The request message.</param>
    /// <param name="context">The server call context.</param>
    /// <param name="continuation">The continuation for the next interceptor or service method.</param>
    /// <returns>A task representing the response.</returns>
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            HandleException(ex, context);
            throw;
        }
    }

    /// <summary>
    /// Intercepts server streaming calls to log exceptions.
    /// </summary>
    /// <typeparam name="TRequest">The request message type.</typeparam>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="request">The request message.</param>
    /// <param name="responseStream">The response stream.</param>
    /// <param name="context">The server call context.</param>
    /// <param name="continuation">The continuation for the next interceptor or service method.</param>
    /// <returns>A task representing the completion of the streaming call.</returns>
    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception ex)
        {
            HandleException(ex, context);
            throw;
        }
    }

    /// <summary>
    /// Intercepts client streaming calls to log exceptions.
    /// </summary>
    /// <typeparam name="TRequest">The request message type.</typeparam>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="requestStream">The request stream.</param>
    /// <param name="context">The server call context.</param>
    /// <param name="continuation">The continuation for the next interceptor or service method.</param>
    /// <returns>A task representing the response.</returns>
    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(requestStream, context);
        }
        catch (Exception ex)
        {
            HandleException(ex, context);
            throw;
        }
    }

    /// <summary>
    /// Intercepts duplex streaming calls to log exceptions.
    /// </summary>
    /// <typeparam name="TRequest">The request message type.</typeparam>
    /// <typeparam name="TResponse">The response message type.</typeparam>
    /// <param name="requestStream">The request stream.</param>
    /// <param name="responseStream">The response stream.</param>
    /// <param name="context">The server call context.</param>
    /// <param name="continuation">The continuation for the next interceptor or service method.</param>
    /// <returns>A task representing the completion of the streaming call.</returns>
    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (Exception ex)
        {
            HandleException(ex, context);
            throw;
        }
    }

    private void HandleException(Exception ex, ServerCallContext context)
    {
        switch (ex)
        {
            case RpcException rpcEx:
                _logger.LogWarning(
                    rpcEx,
                    "gRPC call to {Method} failed with status {StatusCode}: {Detail}",
                    context.Method,
                    rpcEx.StatusCode,
                    rpcEx.Status.Detail);
                break;

            case ArgumentException argEx:
                _logger.LogWarning(
                    argEx,
                    "Invalid argument for gRPC call to {Method}: {Message}",
                    context.Method,
                    argEx.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, argEx.Message));

            case InvalidOperationException invOpEx:
                _logger.LogWarning(
                    invOpEx,
                    "Invalid operation for gRPC call to {Method}: {Message}",
                    context.Method,
                    invOpEx.Message);
                throw new RpcException(new Status(StatusCode.FailedPrecondition, invOpEx.Message));

            default:
                _logger.LogError(
                    ex,
                    "Unhandled exception in gRPC call to {Method}",
                    context.Method);
                throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred"));
        }
    }
}

// <copyright file="TestServiceImpl.cs" company="Your Company">
// Copyright (c) Your Company. All rights reserved.
// </copyright>

namespace Jerry.Library.Grpc.Tests.Services;

using global::Grpc.Core;

/// <summary>
/// Test implementation of TestService for interceptor testing.
/// </summary>
internal class TestServiceImpl : TestService.TestServiceBase
{
    /// <summary>
    /// Echoes the incoming message back to the caller.
    /// </summary>
    /// <param name="request">The echo request.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>The echo response.</returns>
    public override Task<EchoResponse> Echo(EchoRequest request, ServerCallContext context)
    {
        return Task.FromResult(new EchoResponse { Message = request.Message });
    }

    /// <summary>
    /// Throws an exception based on the request parameters.
    /// </summary>
    /// <param name="request">The exception request.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>Never returns - always throws.</returns>
    public override Task<ExceptionResponse> ThrowException(ExceptionRequest request, ServerCallContext context)
    {
        throw request.ExceptionType switch
        {
            "ArgumentException" => new ArgumentException(request.Message),
            "InvalidOperationException" => new InvalidOperationException(request.Message),
            "RpcException" => new RpcException(new Status(StatusCode.NotFound, request.Message)),
            _ => new Exception(request.Message),
        };
    }
}

namespace Jerry.Library.Grpc.Tests.Fixtures;

using global::Grpc.AspNetCore.Server;
using global::Grpc.Core;
using global::Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// xUnit fixture for gRPC server testing. Provides a simple API for starting an in-process
/// gRPC server with custom services, interceptors, and mock dependencies.
/// </summary>
/// <remarks>
/// Use with IClassFixture&lt;GrpcServerFixture&gt; to get one server instance per test class.
/// The server starts lazily on first access to Channel or CreateClient.
/// </remarks>
public class GrpcServerFixture : IDisposable
{
    private readonly object _lock = new();
    private readonly List<Action<IServiceCollection>> _serviceConfigurators = new();
    private readonly List<Action<GrpcServiceOptions>> _grpcConfigurators = new();
    private readonly List<Type> _grpcServiceTypes = new();

    private WebApplication? _app;
    private GrpcChannel? _channel;
    private bool _isStarted;

    static GrpcServerFixture()
    {
        // Enable HTTP/2 without TLS for testing
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    }

    /// <summary>
    /// Gets the gRPC channel for creating clients. The server starts automatically on first access.
    /// </summary>
    public GrpcChannel Channel
    {
        get
        {
            EnsureServerStarted();
            return _channel!;
        }
    }

    /// <summary>
    /// Gets the server URL. The server starts automatically on first access.
    /// </summary>
    public string ServerUrl
    {
        get
        {
            EnsureServerStarted();
            return _app!.Urls.First();
        }
    }

    /// <summary>
    /// Configures the service collection before the server starts.
    /// </summary>
    /// <param name="configure">Action to configure services (e.g., add mocks, singletons).</param>
    /// <returns>This fixture for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called after server has started.</exception>
    public GrpcServerFixture ConfigureServices(Action<IServiceCollection> configure)
    {
        ThrowIfStarted();
        _serviceConfigurators.Add(configure);
        return this;
    }

    /// <summary>
    /// Configures gRPC options before the server starts.
    /// </summary>
    /// <param name="configure">Action to configure gRPC options (e.g., add interceptors, set message size).</param>
    /// <returns>This fixture for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called after server has started.</exception>
    public GrpcServerFixture ConfigureGrpc(Action<GrpcServiceOptions> configure)
    {
        ThrowIfStarted();
        _grpcConfigurators.Add(configure);
        return this;
    }

    /// <summary>
    /// Registers a gRPC service implementation to be hosted by the server.
    /// </summary>
    /// <typeparam name="TService">The gRPC service implementation type.</typeparam>
    /// <returns>This fixture for fluent chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if called after server has started.</exception>
    public GrpcServerFixture AddGrpcService<TService>() where TService : class
    {
        ThrowIfStarted();
        _grpcServiceTypes.Add(typeof(TService));
        return this;
    }

    /// <summary>
    /// Creates a gRPC client for the specified service.
    /// </summary>
    /// <typeparam name="TClient">The gRPC client type (must inherit from ClientBase).</typeparam>
    /// <returns>A configured client instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the client type doesn't have a constructor accepting ChannelBase.</exception>
    public TClient CreateClient<TClient>() where TClient : ClientBase<TClient>
    {
        var constructor = typeof(TClient).GetConstructor(new[] { typeof(ChannelBase) });

        if (constructor == null)
        {
            throw new InvalidOperationException(
                $"Client type {typeof(TClient).Name} does not have a constructor accepting ChannelBase.");
        }

        return (TClient)constructor.Invoke(new object[] { Channel });
    }

    /// <summary>
    /// Disposes the gRPC channel and stops the server.
    /// </summary>
    public void Dispose()
    {
        _channel?.Dispose();
        _channel = null;

        if (_app != null)
        {
            _app.StopAsync().Wait();
            _app.DisposeAsync().AsTask().Wait();
            _app = null;
        }
    }

    private void EnsureServerStarted()
    {
        if (_isStarted)
        {
            return;
        }

        lock (_lock)
        {
            if (_isStarted)
            {
                return;
            }

            if (_grpcServiceTypes.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one gRPC service must be registered using AddGrpcService<T>() before starting the server.");
            }

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

            // Apply custom service configurations
            foreach (var configurator in _serviceConfigurators)
            {
                configurator(builder.Services);
            }

            // Add gRPC with custom configurations
            var grpcBuilder = builder.Services.AddGrpc(options =>
            {
                foreach (var configurator in _grpcConfigurators)
                {
                    configurator(options);
                }
            });

            _app = builder.Build();

            // Map all registered gRPC services
            foreach (var serviceType in _grpcServiceTypes)
            {
                var mapMethod = typeof(GrpcEndpointRouteBuilderExtensions)
                    .GetMethod(nameof(GrpcEndpointRouteBuilderExtensions.MapGrpcService))!
                    .MakeGenericMethod(serviceType);

                mapMethod.Invoke(null, new object[] { _app });
            }

            _app.StartAsync().Wait();

            // Get the actual port and create channel
            var address = _app.Urls.First();
            _channel = GrpcChannel.ForAddress(address);

            _isStarted = true;
        }
    }

    private void ThrowIfStarted()
    {
        if (_isStarted)
        {
            throw new InvalidOperationException(
                "Configuration cannot be changed after the server has started.");
        }
    }
}

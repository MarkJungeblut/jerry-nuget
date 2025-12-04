namespace Jerry.Library.Grpc.Tests.Fixtures;

using Jerry.Library.Grpc.Interceptors;
using Jerry.Library.Grpc.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

/// <summary>
/// Specialized fixture for testing ExceptionLoggingInterceptor with a pre-configured mock logger.
/// </summary>
public class ExceptionLoggingTestFixture : GrpcServerFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionLoggingTestFixture"/> class.
    /// </summary>
    public ExceptionLoggingTestFixture()
    {
        MockLogger = Substitute.For<ILogger<ExceptionLoggingInterceptor>>();

        this.ConfigureServices(services => services.AddSingleton(MockLogger))
            .ConfigureGrpc(options => options.Interceptors.Add<ExceptionLoggingInterceptor>())
            .AddGrpcService<TestServiceImpl>();
    }

    /// <summary>
    /// Gets the mock logger instance that can be used for verification in tests.
    /// </summary>
    public ILogger<ExceptionLoggingInterceptor> MockLogger { get; }
}

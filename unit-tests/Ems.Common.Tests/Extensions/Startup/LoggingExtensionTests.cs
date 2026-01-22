namespace Ems.Common.Tests.Extensions.Startup;

using Ems.Common.Extensions.Startup;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Xunit;

public class LoggingExtensionTests
{
    [Fact]
    public void ConfigureCustomLogging_ShouldReturnIHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var result = hostBuilder.ConfigureCustomLogging();

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IHostBuilder>();
        result.Should().BeSameAs(hostBuilder);
    }

    [Fact]
    public void ConfigureCustomLogging_WithDevelopmentEnvironment_ShouldConfigureSerilog()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .UseEnvironment(Environments.Development);

        var result = hostBuilder.ConfigureCustomLogging();

        result.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureCustomLogging_WithProductionEnvironment_ShouldConfigureSerilog()
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .UseEnvironment(Environments.Production);

        var result = hostBuilder.ConfigureCustomLogging();

        result.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureCustomLogging_ShouldAllowChaining()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        var result = hostBuilder
            .ConfigureCustomLogging()
            .ConfigureServices(services => { });

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IHostBuilder>();
    }
}

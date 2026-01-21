namespace AWSService.Tests.Extensions;

using Amazon.SimpleEmail;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AWSService.Extensions;
using AWSService.Services;
using AWSService.Settings;
using AWSService.Tests.Helpers;
using Xunit;

public class AWSServiceExtensionsTests
{
    [Fact]
    public void AddAWSSES_ShouldRegisterServices()
    {
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddAWSSES(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<IAWSSESClientFactory>();
        var client = serviceProvider.GetService<IAmazonSimpleEmailService>();

        factory.Should().NotBeNull();
        factory.Should().BeAssignableTo<IAWSSESClientFactory>();
        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }

    [Fact]
    public void AddAWSSES_ShouldConfigureSettings()
    {
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddAWSSES(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AWSSESSettings>>();

        options.Value.Should().NotBeNull();
        options.Value.FromEmail.Should().Be("test@example.com");
        options.Value.Region.Should().Be("us-east-1");
    }

    [Fact]
    public void AddAWSSES_ShouldValidateSettingsOnStart()
    {
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        services.AddAWSSES(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var act = () => serviceProvider.GetRequiredService<IAWSSESClientFactory>();

        act.Should().NotThrow();
    }

    [Fact]
    public void AddAWSSES_WithCustomConfigSection_ShouldUseProvidedSection()
    {
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration("CustomAwsSes");
        services.AddAWSSES(configuration, "CustomAwsSes");

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AWSSESSettings>>();

        options.Value.Should().NotBeNull();
        options.Value.FromEmail.Should().Be("custom@example.com");
    }

    private static IConfiguration CreateTestConfiguration(string sectionName = "AwsSes")
    {
        var configurationData = new Dictionary<string, string?>
        {
            { $"{sectionName}:FromEmail", sectionName == "AwsSes" ? "test@example.com" : "custom@example.com" },
            { $"{sectionName}:Region", "us-east-1" }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();
    }
}

namespace AWSService.Tests.Extensions;

using AWSService.Services;
using AWSService.Tests.Helpers;
using Amazon.SimpleEmail;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class AWSServiceExtensionsTests
{
    [Fact]
    public void AddAWSSES_ShouldRegisterServices()
    {
        var configuration = AWSTestHelper.CreateTestConfiguration();
        var serviceProvider = AWSTestHelper.BuildServiceProviderWithAWSSES(configuration);

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
        var configuration = AWSTestHelper.CreateTestConfiguration();
        var serviceProvider = AWSTestHelper.BuildServiceProviderWithAWSSES(configuration);
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AWSService.Settings.AWSSESSettings>>();

        options.Value.Should().NotBeNull();
        options.Value.FromEmail.Should().Be("test@example.com");
        options.Value.Region.Should().Be("us-east-1");
    }

    [Fact]
    public void AddAWSSES_ShouldValidateSettingsOnStart()
    {
        var configuration = AWSTestHelper.CreateTestConfiguration();
        var serviceProvider = AWSTestHelper.BuildServiceProviderWithAWSSES(configuration);

        var act = serviceProvider.GetRequiredService<IAWSSESClientFactory>;

        act.Should().NotThrow();
    }

    [Fact]
    public void AddAWSSES_WithCustomConfigSection_ShouldUseProvidedSection()
    {
        var configuration = AWSTestHelper.CreateTestConfiguration("CustomAwsSes");
        var serviceProvider = AWSTestHelper.BuildServiceProviderWithAWSSES(configuration, "CustomAwsSes");
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AWSService.Settings.AWSSESSettings>>();

        options.Value.Should().NotBeNull();
        options.Value.FromEmail.Should().Be("custom@example.com");
    }
}

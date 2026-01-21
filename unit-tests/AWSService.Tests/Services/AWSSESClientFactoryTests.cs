namespace AWSService.Tests.Services;

using Amazon.SimpleEmail;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using AWSService.Services;
using AWSService.Settings;
using AWSService.Tests.Helpers;
using Xunit;

public class AWSSESClientFactoryTests
{
    [Fact]
    public void CreateClient_WithValidSettings_ShouldReturnClient()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        var options = Options.Create(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }

    [Fact]
    public void CreateClient_WithAccessKeyAndSecretKey_ShouldCreateClientWithCredentials()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.AccessKey = "test-access-key";
        settings.SecretKey = "test-secret-key";
        var options = Options.Create(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }

    [Fact]
    public void CreateClient_WithoutCredentials_ShouldCreateClientWithoutCredentials()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.AccessKey = string.Empty;
        settings.SecretKey = string.Empty;
        var options = Options.Create(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }

    [Fact]
    public void CreateClient_WithServiceUrl_ShouldConfigureServiceUrl()
    {
        var serviceUrl = "http://localhost:4566";
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.ServiceURL = serviceUrl;
        var options = Options.Create(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        var config = client.Config;
        config.ServiceURL.Should().StartWith(serviceUrl);
    }

    [Fact]
    public void CreateClient_WithRegion_ShouldConfigureCorrectRegion()
    {
        var region = "us-west-2";
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.Region = region;
        var options = Options.Create(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        var config = client.Config;
        config.RegionEndpoint.SystemName.Should().Be(region);
    }

    [Fact]
    public void CreateClient_ReturnsIAmazonSimpleEmailService()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        var options = Options.Create(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }
}

namespace AWSService.Tests.Services;

using AWSService.Services;
using AWSService.Tests.Helpers;
using Amazon.SimpleEmail;
using FluentAssertions;
using Xunit;

public class AWSSESClientFactoryTests
{
    [Fact]
    public void CreateClient_WithValidSettings_ShouldReturnClient()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        var options = AWSTestHelper.CreateOptions(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }

    [Theory]
    [InlineData("test-access-key", "test-secret-key")]
    [InlineData("", "")]
    public void CreateClient_WithCredentials_ShouldHandleCredentials(string accessKey, string secretKey)
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings(accessKey: accessKey, secretKey: secretKey);
        var options = AWSTestHelper.CreateOptions(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<IAmazonSimpleEmailService>();
    }

    [Fact]
    public void CreateClient_WithServiceUrl_ShouldConfigureServiceUrl()
    {
        var serviceUrl = "http://localhost:4566";
        var settings = AWSTestHelper.CreateTestAWSSESSettings(serviceUrl: serviceUrl);
        var options = AWSTestHelper.CreateOptions(settings);
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
        var settings = AWSTestHelper.CreateTestAWSSESSettings(region: region);
        var options = AWSTestHelper.CreateOptions(settings);
        var factory = new AWSSESClientFactory(options);

        var client = factory.CreateClient();

        client.Should().NotBeNull();
        var config = client.Config;
        config.RegionEndpoint.SystemName.Should().Be(region);
    }
}

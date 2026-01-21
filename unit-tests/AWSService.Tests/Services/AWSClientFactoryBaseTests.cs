namespace AWSService.Tests.Services;

using Amazon;
using Amazon.Runtime;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using AWSService.Services;
using AWSService.Settings;
using AWSService.Tests.Helpers;
using Xunit;

public class AWSClientFactoryBaseTests
{
    [Fact]
    public void CreateClient_CallsCreateClientCore_WithCorrectConfig()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        var options = Options.Create(settings);
        var testFactory = new TestAWSClientFactory(options);

        var client = testFactory.CreateClient();

        client.Should().NotBeNull();
        testFactory.CreateClientCoreCalled.Should().BeTrue();
        testFactory.LastConfig.Should().NotBeNull();
    }

    [Fact]
    public void CreateClient_WithServiceUrl_PassesToConfig()
    {
        var serviceUrl = "http://localhost:4566";
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.ServiceURL = serviceUrl;
        var options = Options.Create(settings);
        var testFactory = new TestAWSClientFactory(options);

        var client = testFactory.CreateClient();

        client.Should().NotBeNull();
        testFactory.LastConfig.Should().NotBeNull();
        testFactory.LastConfig!.ServiceURL.Should().StartWith(serviceUrl);
    }

    [Fact]
    public void CreateClient_WithRegion_ConfiguresRegionEndpoint()
    {
        var region = "eu-west-1";
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.Region = region;
        var options = Options.Create(settings);
        var testFactory = new TestAWSClientFactory(options);

        var client = testFactory.CreateClient();

        client.Should().NotBeNull();
        testFactory.LastConfig.Should().NotBeNull();
        testFactory.LastConfig!.RegionEndpoint.SystemName.Should().Be(region);
    }

    [Fact]
    public void CreateClient_WithCredentials_PassesToCreateClientCore()
    {
        var settings = AWSTestHelper.CreateTestAWSSESSettings();
        settings.AccessKey = "test-key";
        settings.SecretKey = "test-secret";
        var options = Options.Create(settings);
        var testFactory = new TestAWSClientFactory(options);

        var client = testFactory.CreateClient();

        client.Should().NotBeNull();
        testFactory.LastAccessKey.Should().Be("test-key");
        testFactory.LastSecretKey.Should().Be("test-secret");
    }
}

public class TestAWSClientFactory : AWSClientFactoryBase<TestClient, TestConfig, AWSSESSettings>
{
    public bool CreateClientCoreCalled { get; private set; }
    public TestConfig? LastConfig { get; private set; }
    public string? LastAccessKey { get; private set; }
    public string? LastSecretKey { get; private set; }

    public TestAWSClientFactory(IOptions<AWSSESSettings> settings) : base(settings)
    {
    }

    protected override TestClient CreateClientCore(TestConfig config, string? accessKey, string? secretKey)
    {
        CreateClientCoreCalled = true;
        LastConfig = config;
        LastAccessKey = accessKey;
        LastSecretKey = secretKey;
        return new TestClient();
    }
}

public class TestClient
{
}

public class TestConfig : ClientConfig
{
    public TestConfig()
    {
        RegionEndpoint = RegionEndpoint.USEast1;
    }

    public override string UserAgent => "TestUserAgent";
    public override string RegionEndpointServiceName => "test";
    public override string ServiceVersion => "1.0";
}

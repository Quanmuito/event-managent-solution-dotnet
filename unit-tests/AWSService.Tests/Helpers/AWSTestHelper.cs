namespace AWSService.Tests.Helpers;

using AWSService.Extensions;
using AWSService.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class AWSTestHelper
{
    public static AWSSESSettings CreateTestAWSSESSettings(
        string fromEmail = "test@example.com",
        string region = "us-east-1",
        string? serviceUrl = null,
        string? accessKey = null,
        string? secretKey = null)
    {
        return new AWSSESSettings
        {
            FromEmail = fromEmail,
            Region = region,
            ServiceURL = serviceUrl,
            AccessKey = accessKey ?? string.Empty,
            SecretKey = secretKey ?? string.Empty
        };
    }

    public static IConfiguration CreateTestConfiguration(string sectionName = "AwsSes")
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

    public static ServiceProvider BuildServiceProviderWithAWSSES(IConfiguration configuration, string configSection = "AwsSes")
    {
        var services = new ServiceCollection();
        services.AddAWSSES(configuration, configSection);
        return services.BuildServiceProvider();
    }

    public static IOptions<AWSSESSettings> CreateOptions(AWSSESSettings settings)
    {
        return Options.Create(settings);
    }
}

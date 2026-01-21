namespace AWSService.Tests.Helpers;

using AWSService.Settings;

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
}

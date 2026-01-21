namespace NotificationService.Tests.Helpers;

using Amazon.SimpleEmail.Model;
using AWSService.Settings;
using Microsoft.Extensions.Options;

public static class NotificationTestHelper
{
    public static AWSSESSettings CreateTestAWSSESSettings(string fromEmail = "test@example.com")
    {
        return new AWSSESSettings
        {
            FromEmail = fromEmail,
            Region = "us-east-1"
        };
    }

    public static IOptions<AWSSESSettings> CreateTestAWSSESSettingsOptions(string fromEmail = "test@example.com")
    {
        return Options.Create(CreateTestAWSSESSettings(fromEmail));
    }

    public static SendEmailResponse CreateTestSendEmailResponse(string messageId = "test-message-id")
    {
        return new SendEmailResponse
        {
            MessageId = messageId
        };
    }
}

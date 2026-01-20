namespace NotificationService.Services;

using Microsoft.Extensions.Logging;

public class PhoneService(ILogger<PhoneService> logger) : IPhoneService
{
    public Task SendAsync(string recipient, string content, CancellationToken cancellationToken)
    {
        logger.LogInformation("SMS sent to {Recipient}", recipient);
        logger.LogDebug("SMS content:\n{Content}", content);
        return Task.CompletedTask;
    }
}

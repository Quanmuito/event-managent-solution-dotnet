namespace NotificationService.Common.Services;

using Microsoft.Extensions.Logging;

public class PhoneService(ILogger<PhoneService> logger) : IPhoneService
{
    public Task SendAsync(string recipient, string content, CancellationToken cancellationToken)
    {
        logger.LogInformation("PHONE SERVICE: SMS sent to {Recipient}", recipient);
        logger.LogDebug("PHONE SERVICE: SMS content:\n{Content}", content);
        return Task.CompletedTask;
    }
}

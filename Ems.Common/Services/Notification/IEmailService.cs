namespace Ems.Common.Services.Notification;

using Microsoft.Extensions.Logging;

public interface IEmailService
{
    Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken);
}

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendAsync(string recipient, string subject, string content, CancellationToken cancellationToken)
    {
        logger.LogInformation("Email sent to {Recipient} with subject: {Subject}", recipient, subject);
        logger.LogDebug("Email content:\n{Content}", content);
        return Task.CompletedTask;
    }
}

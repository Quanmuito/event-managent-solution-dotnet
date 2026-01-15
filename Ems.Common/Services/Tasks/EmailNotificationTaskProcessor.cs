namespace Ems.Common.Services.Tasks;

using Ems.Common.Services.Tasks.Messages;
using Microsoft.Extensions.Logging;

public class EmailNotificationTaskProcessor(ILogger<EmailNotificationTaskProcessor> logger) : ITaskProcessor<EmailNotificationTaskMessage>
{
    public async Task ProcessAsync(EmailNotificationTaskMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing email notification for {Recipient}", message.RecipientEmail);
        await SendEmailAsync(message, cancellationToken);
    }

    protected virtual Task SendEmailAsync(EmailNotificationTaskMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email would be sent to {Recipient} with subject: {Subject}",
            message.RecipientEmail,
            message.Subject
        );
        return Task.CompletedTask;
    }
}

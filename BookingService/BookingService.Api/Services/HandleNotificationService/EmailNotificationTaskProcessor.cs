namespace BookingService.Api.Services;

using Ems.Common.Services.Tasks.Messages;
using Microsoft.Extensions.Logging;

public class EmailNotificationTaskProcessor(ILogger<EmailNotificationTaskProcessor> logger) : Ems.Common.Services.Tasks.EmailNotificationTaskProcessor(logger)
{
    protected override Task SendEmailAsync(EmailNotificationTaskMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "BookingService: Email would be sent to {Recipient} with subject: {Subject}",
            message.RecipientEmail,
            message.Subject
        );

        if (message.Metadata != null && message.Metadata.TryGetValue("BookingId", out var bookingId))
        {
            logger.LogInformation("BookingService: Processing notification for Booking ID: {BookingId}", bookingId);
        }

        return base.SendEmailAsync(message, cancellationToken);
    }
}

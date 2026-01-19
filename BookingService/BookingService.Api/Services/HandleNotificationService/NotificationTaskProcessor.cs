namespace BookingService.Api.Services;

using BookingService.Api.Messages;
using BookingService.Api.Models;
using BookingService.Api.Utils;
using EventService.Data.Repositories;
using Ems.Common.Services.Tasks;

public class NotificationTaskProcessor(
    IEventRepository eventRepository,
    ILogger<NotificationTaskProcessor> logger
) : ITaskProcessor<NotificationTaskMessage>
{
    public async Task ProcessAsync(NotificationTaskMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing notification for BookingId: {BookingId}, Operation: {Operation}", message.Booking.Id, message.Operation);

        var subject = GetEmailSubject(message.Operation);
        var content = await ComposeEmailContent(message.Booking, message.Operation, cancellationToken);

        logger.LogInformation("Email Subject: {Subject}", subject);
        logger.LogInformation("Email Content:\n{Content}", content);
    }

    private static string GetEmailSubject(string operation)
    {
        return operation switch
        {
            BookingOperation.QueueEnrolled => "Enrolled to Queue",
            _ => $"Booking {operation}"
        };
    }

    private async Task<string> ComposeEmailContent(BookingDto booking, string operation, CancellationToken cancellationToken)
    {
        var content = $"Dear {booking.Name},\n\n";

        content += operation switch
        {
            BookingOperation.Registered => "Your booking has been successfully registered.\n\n",
            BookingOperation.QueueEnrolled => "Your booking has been added to the queue.\n\n",
            _ => $"Your booking has been {operation}.\n\n"
        };

        content += "Booking Details:\n";
        content += $"  Booking ID: {booking.Id}\n";
        content += $"  Name: {booking.Name}\n";
        content += $"  Email: {booking.Email}\n";
        content += $"  Phone: {booking.Phone}\n";

        try
        {
            var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
            content += "\nEvent Details:\n";
            content += $"  Event Title: {eventEntity.Title}\n";
            content += $"  Hosted By: {eventEntity.HostedBy}\n";
            content += $"  Start Time: {eventEntity.TimeStart:yyyy-MM-dd HH:mm:ss} UTC\n";
            content += $"  End Time: {eventEntity.TimeEnd:yyyy-MM-dd HH:mm:ss} UTC\n";
            if (!string.IsNullOrWhiteSpace(eventEntity.Details))
            {
                content += $"  Details: {eventEntity.Details}\n";
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch event details for EventId: {EventId}", booking.EventId);
        }

        content += "\nBest regards.\n";

        return content;
    }
}

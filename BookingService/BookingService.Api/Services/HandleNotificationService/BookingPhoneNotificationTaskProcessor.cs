namespace BookingService.Api.Services;

using BookingService.Api.Models;
using BookingService.Api.Utils;
using EventService.Data.Repositories;
using NotificationService.Services;
using NotificationService.Services.Tasks;
using Microsoft.Extensions.Logging;

public class BookingPhoneNotificationTaskProcessor(
    IPhoneService phoneService,
    IEventRepository eventRepository,
    ILogger<BookingPhoneNotificationTaskProcessor> logger
) : PhoneNotificationTaskProcessor<BookingDto>(phoneService, logger)
{
    protected override string GetRecipient(BookingDto data)
    {
        return data.Phone;
    }

    protected override async Task<string> ComposeContentAsync(BookingDto booking, string operation, CancellationToken cancellationToken)
    {
        var content = $"Hi {booking.Name},\n\n";

        content += operation switch
        {
            BookingOperation.Registered => "Your booking is confirmed.\n",
            BookingOperation.QueueEnrolled => "Your booking is in queue.\n",
            BookingOperation.Confirmed => "Your booking has been confirmed.\n",
            BookingOperation.Canceled => "Your booking has been canceled.\n",
            _ => $"Your booking has been {operation}.\n"
        };

        content += $"Booking ID: {booking.Id}\n";

        try
        {
            var eventEntity = await eventRepository.GetByIdAsync(booking.EventId, cancellationToken);
            content += $"Event: {eventEntity.Title}\n";
            content += $"Start: {eventEntity.TimeStart:yyyy-MM-dd HH:mm} UTC\n";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch event details for EventId: {EventId}", booking.EventId);
        }

        return content;
    }

    protected override Task HandleBusinessLogicAsync(BookingDto booking, string operation, CancellationToken cancellationToken)
    {
        logger.LogInformation("[TASK - PROCESSING] Handling business logic for phone notification: BookingId={BookingId}, Operation={Operation}", booking.Id, operation);
        return Task.CompletedTask;
    }
}

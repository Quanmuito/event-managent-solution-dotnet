namespace BookingService.Api.Services;

using MongoDB.Driver;
using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;
using Ems.Common.Services.Tasks;
using Ems.Common.Services.Tasks.Messages;

public class HandleBookingService(
    IBookingRepository bookingRepository,
    IQrCodeRepository qrCodeRepository,
    ITaskQueue<QrCodeTaskMessage> taskQueue,
    ITaskQueue<EmailNotificationTaskMessage> emailTaskQueue)
{
    public async Task<BookingDto> GetById(string id, CancellationToken cancellationToken)
    {
        var bookingEntity = await bookingRepository.GetByIdAsync(id, cancellationToken);
        var qrCode = await qrCodeRepository.GetByBookingIdAsync(id, cancellationToken);
        var bookingDto = new BookingDto(bookingEntity!)
        {
            QrCodeData = qrCode?.QrCodeData
        };
        return bookingDto;
    }

    public async Task<Booking> Create(CreateBookingDto createDto, CancellationToken cancellationToken)
    {
        var newBooking = new Booking
        {
            EventId = createDto.EventId,
            Status = createDto.Status,
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone,
            CreatedAt = DateTime.UtcNow
        };
        var booking = await bookingRepository.CreateAsync(newBooking, cancellationToken);

        if (booking.Id == null)
            throw new InvalidOperationException("Failed to create booking.");

        if (booking.Status == BookingStatus.Registered)
            await taskQueue.EnqueueAsync(new QrCodeTaskMessage(booking.Id), cancellationToken);

        await SendEmailNotificationAsync(booking, "Create", cancellationToken);

        return booking;
    }

    public async Task<Booking> Update(string id, UpdateBookingDto updateDto, CancellationToken cancellationToken)
    {
        var updates = new List<UpdateDefinition<Booking>>();

        if (updateDto.Status != null)
            updates.Add(Builders<Booking>.Update.Set(b => b.Status, updateDto.Status));

        if (updateDto.Name != null)
            updates.Add(Builders<Booking>.Update.Set(b => b.Name, updateDto.Name));

        if (updateDto.Email != null)
            updates.Add(Builders<Booking>.Update.Set(b => b.Email, updateDto.Email));

        if (updateDto.Phone != null)
            updates.Add(Builders<Booking>.Update.Set(b => b.Phone, updateDto.Phone));

        if (updates.Count == 0)
            throw new ArgumentException("No valid fields to update.");

        updates.Add(Builders<Booking>.Update.Set(b => b.UpdatedAt, DateTime.UtcNow));

        var updateDef = Builders<Booking>.Update.Combine(updates);

        var result = await bookingRepository.UpdateAsync(id, updateDef, cancellationToken);

        await SendEmailNotificationAsync(result!, "Update", cancellationToken);

        return result;
    }

    public async Task<Booking> Cancel(string id, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(id, cancellationToken);

        if (booking!.Status == BookingStatus.Canceled)
            throw new InvalidOperationException("Booking is already canceled.");

        if (!BookingStatus.AllowedStatusesForCancellation.Contains(booking.Status))
        {
            throw new InvalidOperationException(
                $"Cannot cancel booking with status '{booking.Status}'. " +
                $"Only bookings with statuses: {string.Join(", ", BookingStatus.AllowedStatusesForCancellation)} can be canceled.");
        }

        var updateDef = Builders<Booking>.Update
            .Set(b => b.Status, BookingStatus.Canceled)
            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        var result = await bookingRepository.UpdateAsync(id, updateDef, cancellationToken);

        await SendEmailNotificationAsync(result!, "Cancel", cancellationToken);

        return result;
    }

    public async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(id, cancellationToken);
        var deleted = await bookingRepository.DeleteAsync(id, cancellationToken);

        if (deleted && booking != null)
            await SendEmailNotificationAsync(booking, "Delete", cancellationToken);

        return deleted;
    }

    private async Task SendEmailNotificationAsync(Booking booking, string operation, CancellationToken cancellationToken)
    {
        var (action, timestampLabel, timestamp) = operation switch
        {
            "Create" => ("confirmed", "Created At", booking.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
            "Update" => ("updated", "Updated At", booking.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            "Cancel" => ("cancelled", "Cancelled At", booking.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            "Delete" => ("deleted", "Deleted At", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
            _ => (operation.ToLower(), $"{operation} At", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
        };

        var subject = operation switch
        {
            "Create" => "Booking Confirmation",
            _ => $"Booking {operation}"
        };

        var commonDetails = $"Booking ID: {booking.Id}\nEvent ID: {booking.EventId}\nStatus: {booking.Status}";
        var body = $"Dear {booking.Name},\n\nYour booking has been {action}.\n\n{commonDetails}\n{timestampLabel}: {timestamp} UTC";

        await emailTaskQueue.EnqueueAsync(new EmailNotificationTaskMessage(
            RecipientEmail: booking.Email,
            Subject: subject,
            Body: body,
            ServiceType: "BookingService",
            Metadata: new Dictionary<string, object> { { "BookingId", booking.Id! }, { "Operation", operation } }
        ), cancellationToken);
    }
}

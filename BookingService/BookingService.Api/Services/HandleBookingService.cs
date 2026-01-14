namespace BookingService.Api.Services;

using MongoDB.Driver;
using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Repositories;
using BookingService.Data.Utils;

public class HandleBookingService(IBookingRepository bookingRepository)
{
    public async Task<BookingDto> GetById(string id, CancellationToken cancellationToken)
    {
        var bookingEntity = await bookingRepository.GetByIdAsync(id, cancellationToken);
        return new BookingDto(bookingEntity!);
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
        return await bookingRepository.CreateAsync(newBooking, cancellationToken);
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

        updates.Add(Builders<Booking>.Update.Set(b => b.UpdatedAt, DateTime.UtcNow));

        if (updates.Count == 1)
            throw new ArgumentException("No valid fields to update.");

        var updateDef = Builders<Booking>.Update.Combine(updates);

        var result = await bookingRepository.UpdateAsync(id, updateDef, cancellationToken);
        return result!;
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
        return result!;
    }

    public async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        return await bookingRepository.DeleteAsync(id, cancellationToken);
    }
}

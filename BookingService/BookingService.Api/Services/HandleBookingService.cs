namespace BookingService.Api.Services;

using MongoDB.Driver;
using BookingService.Api.Models;
using BookingService.Data.Models;
using BookingService.Data.Repositories;

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
            CreatedAt = DateTime.UtcNow
        };
        return await bookingRepository.CreateAsync(newBooking, cancellationToken);
    }

    public async Task<Booking> Update(string id, UpdateBookingDto updateDto, CancellationToken cancellationToken)
    {
        var updateDef = Builders<Booking>.Update.Set(b => b.UpdatedAt, DateTime.UtcNow);

        var result = await bookingRepository.UpdateAsync(id, updateDef, cancellationToken);
        return result!;
    }

    public async Task<bool> Delete(string id, CancellationToken cancellationToken)
    {
        return await bookingRepository.DeleteAsync(id, cancellationToken);
    }
}

namespace BookingService.Data.Repositories;

using BookingService.Data.Models;
using BookingService.Data.Utils;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Driver;

public class BookingRepository(MongoDbContext mongoDbContext) : Repository<Booking>(mongoDbContext, "Bookings"), IBookingRepository
{
    public async Task<Booking> ConfirmAsync(string id, CancellationToken cancellationToken)
    {
        var booking = await GetByIdAsync(id, cancellationToken);

        if (booking.Status != BookingStatus.QueuePending)
            throw new InvalidOperationException("Only bookings with QueuePending status can be confirmed.");

        var updateDef = Builders<Booking>.Update
            .Set(b => b.Status, BookingStatus.Registered)
            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        return await UpdateAsync(id, updateDef, cancellationToken);
    }

    public async Task<Booking> CancelAsync(string id, CancellationToken cancellationToken)
    {
        var booking = await GetByIdAsync(id, cancellationToken);

        if (booking.Status == BookingStatus.Canceled)
            throw new InvalidOperationException("Booking is already canceled.");

        var updateDef = Builders<Booking>.Update
            .Set(b => b.Status, BookingStatus.Canceled)
            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        return await UpdateAsync(id, updateDef, cancellationToken);
    }
}

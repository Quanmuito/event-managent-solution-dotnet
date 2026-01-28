namespace BookingService.Data.Repositories;

using BookingService.Data.Models;
using BookingService.Data.Utils;
using DatabaseService;
using DatabaseService.Repositories;
using MongoDB.Driver;

public class BookingRepository(MongoDbContext mongoDbContext) : Repository<Booking>(mongoDbContext, "Bookings"), IBookingRepository
{
    public async Task<Booking> PromoteInQueueAsync(string eventId, CancellationToken cancellationToken)
    {
        var filter = Builders<Booking>.Filter.And(
            Builders<Booking>.Filter.Eq(b => b.Status, BookingStatus.QueueEnrolled),
            Builders<Booking>.Filter.Eq(b => b.EventId, eventId)
        );

        var sort = Builders<Booking>.Sort.Ascending(b => b.CreatedAt);

        var booking = await Collection
            .Find(filter)
            .Sort(sort)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"No booking found in queue for event '{eventId}'.");

        if (booking.Status != BookingStatus.QueueEnrolled)
            throw new InvalidOperationException("Only bookings with QueueEnrolled status can be promoted in queue.");

        var updateDef = Builders<Booking>.Update
            .Set(b => b.Status, BookingStatus.QueuePending)
            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        return await UpdateAsync(booking.Id!, updateDef, cancellationToken);
    }

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
